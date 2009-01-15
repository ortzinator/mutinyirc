using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlamingIRC
{
    public interface INotifyUpdate
    {
        event EventHandler Updated;
    }
}
