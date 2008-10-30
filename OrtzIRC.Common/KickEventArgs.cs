namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class KickEventArgs : EventArgs
    {
        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Kickee { get; private set; }
        public string Reason {get; private set; }

        public KickEventArgs(User user, Channel channel, string kickee, string reason)
        {
            this.User = user;
            this.Channel = channel;
            this.Kickee = kickee;
            this.Reason = reason;
        }
    }
}
