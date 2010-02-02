namespace FlamingIRC
{
    using System;
    using System.Threading;

    public static class ThreadHelper
    {
        public static void InvokeAfter(TimeSpan time, TimerCallback method)
        {
            if (method == null) throw new ArgumentException("Method parameter cannot be null");

            new Timer(method, new object(), time, TimeSpan.FromMilliseconds(Timeout.Infinite));
        }
    }
}