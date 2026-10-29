using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MutinyIRC.Common;

namespace MutinyIRC.UI;

public sealed class IrcSettingsManager
{
    private static IrcSettingsManager? Singleton;
    private static readonly string DefaultSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "MutinyIRC", "servers.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    // The file this manager reads and writes. It belongs to the instance, not to the type, so
    // a manager built for a test cannot reach the user's real servers.json however it is used.
    private readonly string _settingsPath;

    public NetworkSettingsList Networks { get; private set; }

    /// <summary>
    /// Builds an empty manager over <paramref name="settingsPath"/>, having read nothing from
    /// disk. Internal rather than private so a test can hand one to a view model instead of
    /// letting it reach the singleton, and can point it at a file of its own.
    /// </summary>
    internal IrcSettingsManager(string settingsPath)
    {
        _settingsPath = settingsPath;
        Networks = new NetworkSettingsList();
    }

    public static IrcSettingsManager Instance
    {
        get
        {
            if (Singleton != null) return Singleton;
            Singleton = new IrcSettingsManager(DefaultSettingsPath);
            Singleton.Load();
            return Singleton;
        }
    }

    public NetworkSettings GetOrAddNetwork(string networkName) => Networks.GetOrAddNetwork(networkName);

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            string json = JsonSerializer.Serialize(Networks, JsonOptions);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not save IRC settings to disk: {ex.Message}");
            Debug.WriteLine($"IrcSettingsManager.Save failed: {ex}");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                LoadDefaults();
                return;
            }

            string json = File.ReadAllText(_settingsPath);
            var networks = JsonSerializer.Deserialize<List<NetworkSettings>>(json, JsonOptions);
            if (networks == null)
            {
                LoadDefaults();
                return;
            }

            // Network is [JsonIgnore], so the back-reference every host-to-network lookup relies
            // on has to be rebuilt after deserializing.
            foreach (var network in networks)
                foreach (var server in network.Servers)
                    server.Network = network;

            FoldLegacyEntryPointFields(networks);
            Networks.ReplaceAll(networks);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not load IRC settings: {ex.Message}");
            Debug.WriteLine($"IrcSettingsManager.Load failed: {ex}");
            LoadDefaults();
        }
    }

    /// <summary>
    /// Moves the fields that used to sit on an entry point up onto its network. A file written
    /// before one entry point meant one port, one nick and one connection still loads: the port
    /// takes the first of the old candidate list, the network autoconnects if any of its hosts
    /// did, and the first host that named a nick names it for the network. Nothing writes these
    /// legacy fields again, so the next save drops them.
    /// </summary>
    internal static void FoldLegacyEntryPointFields(IEnumerable<NetworkSettings> networks)
    {
        foreach (NetworkSettings network in networks)
        {
            foreach (ServerSettings entryPoint in network.Servers)
            {
                if (entryPoint.Ports != null)
                    entryPoint.Port = ServerSettings.FirstPortOf(entryPoint.Ports);
                entryPoint.Ports = null;

                network.AutoConnect |= entryPoint.AutoConnect;
                entryPoint.AutoConnect = false;

                if (string.IsNullOrWhiteSpace(network.Nick))
                    network.Nick = entryPoint.Nick;
                entryPoint.Nick = null;
            }
        }
    }

    private void LoadDefaults()
    {
        var net = new NetworkSettings("Libera") { AutoConnect = true };
        net.AddServer(new ServerSettings("irc.libera.chat", ServerSettings.DefaultPort, false));
        net.AddChannel(new ChannelSettings("#MutinyIRC", true));
        Networks.ReplaceAll(new[] { net });
    }

    /// <summary>
    /// The networks to dial at startup. One network gives one connection, so this returns
    /// networks and not a flat scan of every entry point.
    /// </summary>
    public List<NetworkSettings> GetAutoConnectNetworks()
    {
        var tmp = new List<NetworkSettings>();
        foreach (var network in Networks)
            if (network.AutoConnect && network.Servers.Count > 0)
                tmp.Add(network);
        return tmp;
    }

    /// <summary>
    /// Finds the saved network a connection belongs to. A hostname belongs to at most one
    /// network, so the first entry point that matches settles it.
    /// </summary>
    public NetworkSettings? GetNetwork(Server server) => GetServer(server)?.Network;

    /// <summary>Stops the network this connection belongs to being dialled at startup.</summary>
    public void DisableAutoConnect(Server server)
    {
        var network = GetNetwork(server);
        if (network != null)
            network.AutoConnect = false;
    }

    /// <summary>
    /// Gives the certificate trust settings for <paramref name="host"/>, from the network that
    /// lists it. A host that no saved network lists is verified.
    /// </summary>
    /// <remarks>
    /// This reads the settings, which only the UI thread changes. Call it on the UI thread.
    /// </remarks>
    public CertificateTrust GetCertificateTrust(string host)
    {
        NetworkSettings? network = GetServer(host)?.Network;
        if (network == null)
            return CertificateTrust.Default;

        return new CertificateTrust { VerifyCertificates = network.VerifyCertificates };
    }

    private ServerSettings? GetServer(Server server) => GetServer(server.Url);

    private ServerSettings? GetServer(string host) =>
        Networks.SelectMany(network => network.Servers)
                .FirstOrDefault(entryPoint =>
                    entryPoint.Url.Equals(host, StringComparison.OrdinalIgnoreCase));
}
