namespace FlamingIRC
{
    using System;

    public class UserMessageEventArgs : EventArgs
    {
        public User User;
        public string Message;

        public UserMessageEventArgs(User user, string message)
        {
            User = user;
            Message = message;
        }
    }
}