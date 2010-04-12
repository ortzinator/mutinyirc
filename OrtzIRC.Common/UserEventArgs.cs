namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class UserEventArgs : EventArgs
    {
        public UserEventArgs(User user)
        {
            User = user;
        }

        public User User { get; private set; }
    }
}
