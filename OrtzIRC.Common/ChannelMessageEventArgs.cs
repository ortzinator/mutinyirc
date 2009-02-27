namespace OrtzIRC
{
    using System;
    using FlamingIRC;
    using OrtzIRC.Common;

    public class ChannelMessageEventArgs : EventArgs
    {
        public ChannelMessageEventArgs(User user, Channel channel, string message)
        {
            User = user;
            Channel = channel;
            Message = message;
        }

        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Message { get; private set; }
    }
}
