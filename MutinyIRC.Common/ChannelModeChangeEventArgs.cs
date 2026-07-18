using System;
using System.Collections.Generic;
using FlamingIRC;

namespace MutinyIRC.Common;

public class ChannelModeChangeEventArgs : EventArgs
{
    public ChannelModeChangeEventArgs(User user, Channel channel, IEnumerable<ChannelModeInfo> modes, string raw)
    {
        User = user;
        Channel = channel;
        Modes = modes;
        Raw = raw;
    }

    public User User { get; private set; }
    public Channel Channel { get; private set; }
    public IEnumerable<ChannelModeInfo> Modes { get; private set; }
    public string Raw { get; private set; }
}
