namespace OrtzIRC
{
    using System;
    using FlamingIRC;

    public class PrivateMessageEventArgs : EventArgs
    {
        public PrivateMessageEventArgs(User user, string message)
        {
            User = user;
            Message = message;
        }

        public User User { get; private set; }
        public string Message { get; private set; }
    }
}
