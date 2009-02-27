namespace OrtzIRC.Common
{
    using System;

    public class DataEventArgs<T> : EventArgs
    {
        public DataEventArgs(T data)
        {
            Data = data;
        }

        public T Data { get; private set; }
    }
}
