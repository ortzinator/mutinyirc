namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using Sharkbite.Irc;

    public class ChannelModeChangeEventArgs : EventArgs
    {
        public Nick Nick { get; private set; }
        public Channel Channel { get; private set; }
        public IEnumerable<ChannelModeInfo> Modes { get; private set; }
        public string Raw { get; private set; }

        public ChannelModeChangeEventArgs(Nick nick, Channel channel, IEnumerable<ChannelModeInfo> modes, string raw)
        {
            this.Nick = nick;
            this.Channel = channel;
            this.Modes = modes;
            this.Raw = raw;
        }
    }
}
