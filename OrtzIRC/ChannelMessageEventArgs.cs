namespace OrtzIRC
{
    using System;

    public class ChannelMessageEventArgs : EventArgs
    {
        public Nick Nick { get; private set; }
        public Channel Channel { get; private set; }
        public string Message { get; private set; }

        public ChannelMessageEventArgs(Nick nick, Channel channel, string message)
        {
            this.Nick = nick;
            this.Channel = channel;
            this.Message = message;
        }
    }
}
