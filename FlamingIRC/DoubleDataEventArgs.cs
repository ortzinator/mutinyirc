namespace FlamingIRC
{
    using System;

    public class FlamingDoubleDataEventArgs<TFirst, TSecond> : EventArgs
    {
        public TFirst First { get; private set; }

        public TSecond Second { get; private set; }

        public FlamingDoubleDataEventArgs(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }
    }
}