using System;
using System.Collections.Generic;
using FlamingIRC;
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

    /// <summary>
    /// The nick to register with on this network, or null to use <see cref="AppSettings.FirstNick"/>.
    /// Identity belongs to the network and not to the host, so failing over to another entry point
    /// cannot change who you are.
    /// </summary>
    public string? Nick { get; set; }

    /// <summary>The nick to try when <see cref="Nick"/> is taken, or null for
    /// <see cref="AppSettings.SecondNick"/>.</summary>
    public string? AlternateNick { get; set; }

    /// <summary>Whether to dial this network at startup. One network gives one connection, so the
    /// flag belongs here and not on each entry point.</summary>
    public bool AutoConnect { get; set; }

    /// <summary>
    /// The "Verify TLS certificates" switch. When it is off, a connection to any of this
    /// network's entry points accepts a certificate that fails the normal check. On by default,
    /// also for a file written before the switch existed.
    /// </summary>
    public bool VerifyCertificates { get; set; } = true;

    // A servers.json with "Servers": null would otherwise overwrite the list with null and make
    // every reader null-check it. Absent means empty, so say that once, here.
    private List<ServerSettings> _servers = new();

    /// <summary>The entry points, in the order they are dialled.</summary>
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

    /// <summary>
    /// Moves an entry point <paramref name="delta"/> places along the failover order. A move that
    /// would leave the list does nothing, so the caller does not have to check the ends.
    /// </summary>
    public bool MoveServer(ServerSettings server, int delta)
    {
        int from = Servers.IndexOf(server);
        if (from < 0)
            return false;

        int to = from + delta;
        if (to < 0 || to >= Servers.Count)
            return false;

        Servers.RemoveAt(from);
        Servers.Insert(to, server);
        return true;
    }

    /// <summary>
    /// Builds the dial candidates for this network, one per entry point, in failover order.
    /// </summary>
    /// <remarks>
    /// The port is written after the constructor because
    /// <see cref="ConnectionArgs(string, string, bool)"/> hardcodes 6667. Without this the saved
    /// port is dropped, and a TLS entry point on 6697 dials 6667.
    /// </remarks>
    public IReadOnlyList<ConnectionArgs> ToConnectionCandidates()
    {
        string nick = string.IsNullOrWhiteSpace(Nick) ? AppSettings.Instance.FirstNick : Nick;

        var candidates = new List<ConnectionArgs>(Servers.Count);
        foreach (ServerSettings entryPoint in Servers)
        {
            var args = new ConnectionArgs(nick, entryPoint.Url, entryPoint.Ssl)
            {
                Port = entryPoint.Port,
            };
            candidates.Add(args);
        }

        return candidates;
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
        var chan = new ChannelSettings
        {
            Name = data.Name
        };
        AddChannel(chan);
        return chan;
    }

    public void AddChannel(ChannelSettings channel)
    {
        if (Channels.Contains(channel))
            return;

        Channels.Add(channel);
    }

    public bool RemoveChannel(ChannelSettings channel) => Channels.Remove(channel);

    public ChannelSettings? GetChannel(string name)
    {
        foreach (ChannelSettings channel in Channels)
            if (channel.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return channel;
        return null;
    }
}
