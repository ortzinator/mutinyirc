using System;
using System.Collections.Generic;

namespace MutinyIRC.UI;

public class NetworkSettingsList : List<NetworkSettings>
{
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
        Add(net);
        return net;
    }

    public NetworkSettings? GetNetwork(string name)
    {
        foreach (NetworkSettings network in this)
            if (network.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return network;
        return null;
    }
}
