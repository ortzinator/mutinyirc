namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using FlamingIRC;

    public class ChannelModeChangeEventArgs : EventArgs
    {
        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public IEnumerable<ChannelModeInfo> Modes { get; private set; }
        public string Raw { get; private set; }

        public ChannelModeChangeEventArgs(User user, Channel channel, IEnumerable<ChannelModeInfo> modes, string raw)
        {
            this.User = user;
            this.Channel = channel;
            this.Modes = modes;
            this.Raw = raw;
        }
    }
}
