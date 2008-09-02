namespace OrtzIRC
{
    using System;

    public class PartEventArgs : EventArgs
    {
        public Nick Nick { get; private set; }
        public Channel Channel { get; private set; }
        public string Reason { get; private set; }

        public PartEventArgs(Nick nick, Channel channel, string reason)
        {
            this.Nick = nick;
            this.Channel = channel;
            this.Reason = reason;
        }
    }
}
