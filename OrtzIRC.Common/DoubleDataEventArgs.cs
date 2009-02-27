namespace OrtzIRC.Common
{
    using System;

    public class DoubleDataEventArgs<TFirst, TSecond> : EventArgs
    {
        public DoubleDataEventArgs(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        public TFirst First { get; private set; }

        public TSecond Second { get; private set; }
    }
}