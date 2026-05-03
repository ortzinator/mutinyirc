namespace OrtzIRC.Avalonia;

using System.Collections.Generic;

public class NetworkSettingsList : List<NetworkSettings>
{
    public NetworkSettings? AddNetwork(string name)
    {
        var net = new NetworkSettings(name);

        if (Contains(net))
            return null;

        Add(net);
        return net;
    }
}
