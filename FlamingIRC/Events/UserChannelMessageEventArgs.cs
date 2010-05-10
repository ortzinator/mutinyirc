namespace FlamingIRC
{
    using System;

    public class UserChannelMessageEventArgs : EventArgs
    {
        public User User;
        public string Channel;
        public string Message;

        public UserChannelMessageEventArgs(User user, string channel, string message)
        {
            User = user;
            Channel = channel;
            Message = message;
        }
    }
}