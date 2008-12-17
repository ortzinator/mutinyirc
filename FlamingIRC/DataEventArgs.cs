namespace FlamingIRC
{
    using System;

    public class FlamingDataEventArgs<T> : EventArgs
    {
        public T Data { get; private set; }

        public FlamingDataEventArgs(T data)
        {
            this.Data = data;
        }
    }
}