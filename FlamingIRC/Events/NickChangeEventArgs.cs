namespace FlamingIRC
{
    using System;

    public class NickChangeEventArgs : EventArgs
    {
        /// <summary>
        /// The user who is changing his nick.
        /// </summary>
        public User User;

        /// <summary>
        /// The new nickname.
        /// </summary>
        public string NewNick;

        public NickChangeEventArgs(User user, string newNewNick)
        {
            User = user;
            NewNick = newNewNick;
        }
    }
}