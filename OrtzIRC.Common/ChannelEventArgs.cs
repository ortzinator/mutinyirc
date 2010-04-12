namespace OrtzIRC.Common
{
    using System;

    public class ChannelEventArgs : EventArgs
    {
        public ChannelEventArgs(Channel channel)
        {
            Channel = channel;
        }

        public Channel Channel { get; private set; }
    }
}
