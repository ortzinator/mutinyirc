using System;
using System.Collections;
using System.Collections.Generic;

namespace MutinyIRC.UI;

/// <summary>
/// The saved networks. Read-only to callers: every way to change the set goes through a method
/// here, so the rule that a hostname belongs to at most one network cannot be broken from outside.
/// </summary>
/// <remarks>
/// That rule is what lets <see cref="IrcSettingsManager.GetNetwork(MutinyIRC.Common.Server)"/> stop
/// at the first host that matches. Deriving from <see cref="List{T}"/> would leave Add, Insert,
/// AddRange and the indexer setter open and the rule unenforceable.
/// </remarks>
public sealed class NetworkSettingsList : IReadOnlyList<NetworkSettings>
{
    private readonly List<NetworkSettings> _networks = new();

    public int Count => _networks.Count;

    public NetworkSettings this[int index] => _networks[index];

    public IEnumerator<NetworkSettings> GetEnumerator() => _networks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the network that has this name, or adds a network if no network has it.
    /// </summary>
    /// <remarks>
    /// The name does not identify a network, but it is how a new entry point finds the network
    /// it belongs to: a host that reports a known name joins the network that has that name
    /// instead of making a second network. Thus this method always gives you a network.
    /// </remarks>
    public NetworkSettings GetOrAddNetwork(string name)
    {
        NetworkSettings? existing = GetNetwork(name);
        if (existing != null)
            return existing;

        var net = new NetworkSettings(name);
        _networks.Add(net);
        return net;
    }

    public NetworkSettings? GetNetwork(string name)
    {
        foreach (NetworkSettings network in _networks)
            if (network.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return network;
        return null;
    }

    /// <summary>
    /// Removes the network you pass. Identity is by reference, so a network that merely carries
    /// the same name is left alone.
    /// </summary>
    public bool Remove(NetworkSettings network) => _networks.Remove(network);

    /// <summary>
    /// Swaps the whole set for <paramref name="networks"/> in one step, dropping any entry point
    /// a network earlier in the sequence already claims.
    /// </summary>
    /// <remarks>
    /// This is the only way to fill the list, so every network reaching it — whether it came from
    /// disk or from the settings window — passes the same check. A duplicate host is dropped
    /// rather than the network holding it, because the duplicate was already unreachable: a URL
    /// lookup stops at the first network that lists it.
    /// </remarks>
    public void ReplaceAll(IEnumerable<NetworkSettings> networks)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = new List<NetworkSettings>();

        foreach (NetworkSettings network in networks)
        {
            foreach (ServerSettings server in new List<ServerSettings>(network.Servers))
                if (!claimed.Add(server.Url))
                    network.RemoveServer(server);

            accepted.Add(network);
        }

        _networks.Clear();
        _networks.AddRange(accepted);
    }
}
