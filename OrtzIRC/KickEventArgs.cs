namespace OrtzIRC
{
    using System;

    public class KickEventArgs : EventArgs
    {
        public Nick Nick { get; private set; }
        public Channel Channel { get; private set; }
        public string Kickee { get; private set; }
        public string Reason {get; private set; }

        public KickEventArgs(Nick nick, Channel channel, string kickee, string reason)
        {
            this.Nick = nick;
            this.Channel = channel;
            this.Kickee = kickee;
            this.Reason = reason;
        }
    }
}
