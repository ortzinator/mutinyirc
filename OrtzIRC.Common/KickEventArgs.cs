namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class KickEventArgs : EventArgs
    {
        public KickEventArgs(User user, Channel channel, string kickee, string reason)
        {
            User = user;
            Channel = channel;
            Kickee = kickee;
            Reason = reason;
        }

        public User User { get; private set; }
        public Channel Channel { get; private set; }
        public string Kickee { get; private set; }
        public string Reason {get; private set; }
    }
}
