using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnInfo"/>. The server sends one event for each line of
/// its information text.
/// </summary>
public sealed class InfoEventArgs : EventArgs
{
    /// <summary>
    /// One line of information about the server.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// True if this is the last line.
    /// </summary>
    public bool IsLast { get; }

    public InfoEventArgs(string message, bool isLast)
    {
        Message = message;
        IsLast = isLast;
    }
}
