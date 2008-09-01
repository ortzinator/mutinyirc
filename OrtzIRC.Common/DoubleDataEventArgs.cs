namespace OrtzIRC.Common
{
    using System;

    public class DoubleDataEventArgs<TFirst, TSecond> : EventArgs
    {
        public TFirst First { get; private set; }

        public TSecond Second { get; private set; }

        public DoubleDataEventArgs(TFirst first, TSecond second)
        {
            this.First = first;
            this.Second = second;
        }
    }
}