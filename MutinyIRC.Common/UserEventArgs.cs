using System;
using FlamingIRC;

namespace MutinyIRC.Common
{
    public class UserEventArgs : EventArgs
    {
        public UserEventArgs(User user)
        {
            User = user;
        }

        public User User { get; private set; }
    }
}
