namespace OrtzIRC.Common
{
    using System;

    public static class EventUtil
    {
        public static void Fire<TEventArgs>(this EventHandler<TEventArgs> myEvent, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            if (myEvent != null)
            {
                myEvent(sender, e);
            }
        }

        public static void Fire<TEventArgs>(this EventHandler myEvent, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            if (myEvent != null)
            {
                myEvent(sender, e);
            }
        }
    }
}