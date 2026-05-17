namespace OrtzIRC.Avalonia.Controls;

using System;

public class CommandEventArgs : EventArgs
{
    public string Data { get; set; }

    public CommandEventArgs(string data)
    {
        Data = data;
    }
}
