using System;

namespace MutinyIRC.Common;

public class DataEventArgs<T> : EventArgs
{
    public DataEventArgs(T data)
    {
        Data = data;
    }

    public T Data { get; private set; }
}
