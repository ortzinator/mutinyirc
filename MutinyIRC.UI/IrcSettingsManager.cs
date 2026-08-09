using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    public bool RemoveNetwork(NetworkSettings network) => Networks.Remove(network);

    public NetworkSettings? GetNetwork(string name) => Networks.GetNetwork(name);

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

            Networks = new NetworkSettingsList();
            foreach (var network in networks)
            {
                foreach (var server in network.Servers)
                    server.Network = network;
                Networks.Add(network);
            }
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
        Networks = new NetworkSettingsList();
        var net = new NetworkSettings("Libera");
        net.AddServer(new ServerSettings("irc.libera.chat", "Libera", "6667", false));
        net.AddChannel(new ChannelSettings("#MutinyIRC", true));
        Networks.Add(net);
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

    public NetworkSettings? GetNetwork(Server server)
    {
        foreach (NetworkSettings networkSettings in Networks)
            foreach (ServerSettings serverSettings in networkSettings.Servers)
                if (server.Url == serverSettings.Url)
                    return networkSettings;
        return null;
    }

    public void DisableAutoConnect(Server server)
    {
        var settings = GetServer(server);
        if (settings != null)
            settings.AutoConnect = false;
    }

    private ServerSettings? GetServer(Server server)
    {
        foreach (NetworkSettings networkSettings in Networks)
            foreach (ServerSettings serverSettings in networkSettings.Servers)
                if (server.Url == serverSettings.Url)
                    return serverSettings;
        return null;
    }
}
