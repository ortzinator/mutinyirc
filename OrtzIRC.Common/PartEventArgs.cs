namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class PartEventArgs : EventArgs
    {
        public PartEventArgs(User user, Channel channel, string reason)
        {
            User = user;
            Channel = channel;
            Reason = reason;
        }

        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Reason { get; private set; }
    }
}
