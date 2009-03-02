namespace OrtzIRC
{
    using System;
    using OrtzIRC.Common;

    public class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(Channel channel)
        {
            Channel = channel;
        }

        public Channel Channel { get; private set; }
    }
}
