namespace FlamingIRC
{
    using System;

    public class NickChangeEventArgs : EventArgs
    {
        public User User;
        public string NewNick;

        public NickChangeEventArgs(User user, string newNewNick)
        {
            User = user;
            NewNick = newNewNick;
        }
    }
}