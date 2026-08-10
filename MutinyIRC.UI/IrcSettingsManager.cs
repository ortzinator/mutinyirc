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
    private static IrcSettingsManager? instance;
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "MutinyIRC", "servers.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public NetworkSettingsList Networks { get; private set; }

    private IrcSettingsManager()
    {
        Networks = new NetworkSettingsList();
    }

    public static IrcSettingsManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = new IrcSettingsManager();
            instance.Load();
            return instance;
        }
    }

    public NetworkSettings GetOrAddNetwork(string networkName) => Networks.GetOrAddNetwork(networkName);

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            string json = JsonSerializer.Serialize(Networks, JsonOptions);
            File.WriteAllText(SettingsPath, json);
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
            if (!File.Exists(SettingsPath))
            {
                LoadDefaults();
                return;
            }

            string json = File.ReadAllText(SettingsPath);
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

            Networks.ReplaceAll(networks);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not load IRC settings: {ex.Message}");
            Debug.WriteLine($"IrcSettingsManager.Load failed: {ex}");
            LoadDefaults();
        }
    }

    private void LoadDefaults()
    {
        var net = new NetworkSettings("Libera");
        net.AddServer(new ServerSettings("irc.libera.chat", "Libera", "6667", false));
        net.AddChannel(new ChannelSettings("#MutinyIRC", true));
        Networks.ReplaceAll(new[] { net });
    }

    public List<ServerSettings> GetAutoConnectServers()
    {
        var tmp = new List<ServerSettings>();
        foreach (var network in Networks)
            foreach (var server in network.Servers)
                if (server.AutoConnect)
                    tmp.Add(server);
        return tmp;
    }

    /// <summary>
    /// Finds the saved network a connection belongs to. A hostname belongs to at most one
    /// network, so the first entry point that matches settles it.
    /// </summary>
    public NetworkSettings? GetNetwork(Server server) => GetServer(server)?.Network;

    public void DisableAutoConnect(Server server)
    {
        var settings = GetServer(server);
        if (settings != null)
            settings.AutoConnect = false;
    }

    private ServerSettings? GetServer(Server server) =>
        Networks.SelectMany(network => network.Servers)
                .FirstOrDefault(entryPoint =>
                    entryPoint.Url.Equals(server.Url, StringComparison.OrdinalIgnoreCase));
}
