namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class PartEventArgs : EventArgs
    {
        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Reason { get; private set; }

        public PartEventArgs(User user, Channel channel, string reason)
        {
            this.User = user;
            this.Channel = channel;
            this.Reason = reason;
        }
    }
}
