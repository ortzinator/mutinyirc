using System;
using System.Collections.Generic;
using MutinyIRC.Common;

namespace MutinyIRC.UI;

/// <remarks>
/// A network is identified by its entry points, not by its name, and the server can change the
/// name on any connect. Thus this type keeps reference identity: one instance is one network.
/// To find the network for a host, use <see cref="IrcSettingsManager.GetNetwork(Server)"/>.
/// </remarks>
public class NetworkSettings
{
    public NetworkSettings(string name) => Name = name;

    public NetworkSettings() : this("") { }

    public string Name { get; set; }

    // A servers.json with "Servers": null would otherwise overwrite the list with null and make
    // every reader null-check it. Absent means empty, so say that once, here.
    private List<ServerSettings> _servers = new();
    public List<ServerSettings> Servers
    {
        get => _servers;
        set => _servers = value ?? new List<ServerSettings>();
    }

    private List<ChannelSettings> _channels = new();
    public List<ChannelSettings> Channels
    {
        get => _channels;
        set => _channels = value ?? new List<ChannelSettings>();
    }

    public ServerSettings GetRandomServer()
    {
        if (Servers.Count == 0)
            throw new InvalidOperationException("No entry points configured.");

        // The upper bound is exclusive, so it is the count, not the last index.
        return Servers[new Random().Next(Servers.Count)];
    }

    public void AddServer(ServerSettings server)
    {
        if (Servers.Contains(server))
            return;

        server.Network = this;
        Servers.Add(server);
    }

    public ServerSettings AddServer()
    {
        var server = new ServerSettings();
        AddServer(server);
        return server;
    }

    public override string ToString() => Name;

    public bool RemoveServer(ServerSettings serverSettings) => Servers.Remove(serverSettings);

    public ServerSettings? GetServer(string url)
    {
        foreach (ServerSettings server in Servers)
            if (server.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase))
                return server;
        return null;
    }

    public ChannelSettings AddChannel(Channel data)
    {
        var chan = new ChannelSettings();
        chan.Name = data.Name;
        AddChannel(chan);
        return chan;
    }

    public void AddChannel(ChannelSettings channel)
    {
        if (Channels.Contains(channel))
            return;

        Channels.Add(channel);
    }

    public ChannelSettings? GetChannel(string name)
    {
        foreach (ChannelSettings channel in Channels)
            if (channel.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return channel;
        return null;
    }
}
