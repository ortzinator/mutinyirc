namespace FlamingIRC
{
    using System;

    public interface INotifyUpdate
    {
        event EventHandler Updated;
    }
}
