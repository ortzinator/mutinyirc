namespace OrtzIRC
{
    using System;
    using FlamingIRC;

    public class ChannelMessageEventArgs : EventArgs
    {
        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Message { get; private set; }

        public ChannelMessageEventArgs(User user, Channel channel, string message)
        {
            this.User = user;
            this.Channel = channel;
            this.Message = message;
        }
    }
}
