using System;

namespace FlamingIRC;

public interface INotifyUpdate
{
    event EventHandler Updated;
}
