namespace OrtzIRC
{
    using System;
    using FlamingIRC;

    public class PrivateMessageEventArgs : EventArgs
    {
        public User User { get; private set; }
        public string Message { get; private set; }

        public PrivateMessageEventArgs(User user, string message)
        {
            this.User = user;
            this.Message = message;
        }
    }
}
