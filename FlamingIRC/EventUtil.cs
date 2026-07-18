using System;

namespace FlamingIRC;

internal static class EventUtil
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
