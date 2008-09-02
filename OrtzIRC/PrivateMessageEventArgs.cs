namespace OrtzIRC
{
    using System;

    public class PrivateMessageEventArgs : EventArgs
    {
        public Nick Nick { get; private set; }
        public string Message { get; private set; }

        public PrivateMessageEventArgs(Nick nick, string message)
        {
            this.Nick = nick;
            this.Message = message;
        }
    }
}
