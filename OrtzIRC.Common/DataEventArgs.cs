namespace OrtzIRC.Common
{
    using System;

    public class DataEventArgs<T> : EventArgs
    {
        public T Data { get; private set; }

        public DataEventArgs(T data)
        {
            this.Data = data;
        }
    }
}
