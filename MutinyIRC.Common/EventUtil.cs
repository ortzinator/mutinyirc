using System;

namespace MutinyIRC.Common
{
    public static class EventUtil
    {
        public static void Fire<TEventArgs>(this EventHandler<TEventArgs> myEvent, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            myEvent?.Invoke(sender, e);
        }

        public static void Fire<TEventArgs>(this EventHandler myEvent, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            myEvent?.Invoke(sender, e);
        }
    }
}