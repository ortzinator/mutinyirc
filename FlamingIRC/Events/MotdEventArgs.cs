using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnMotd"/>. The server sends one event for each line of
/// its Message of the Day.
/// </summary>
public sealed class MotdEventArgs : EventArgs
{
    /// <summary>
    /// One line of the Message of the Day.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// True if this is the last line.
    /// </summary>
    public bool IsLast { get; }

    public MotdEventArgs(string message, bool isLast)
    {
        Message = message;
        IsLast = isLast;
    }
}
