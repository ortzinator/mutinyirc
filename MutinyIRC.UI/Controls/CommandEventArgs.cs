using System;

namespace MutinyIRC.UI.Controls;

public class CommandEventArgs : EventArgs
{
    public string Data { get; set; }

    public CommandEventArgs(string data)
    {
        Data = data;
    }
}
