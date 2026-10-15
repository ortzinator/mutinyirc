using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnLusers"/>.
/// </summary>
public sealed class LusersEventArgs : EventArgs
{
    /// <summary>
    /// One line of statistics about the size of the network, for example the number of
    /// users or channels.
    /// </summary>
    public string Message { get; }

    public LusersEventArgs(string message) => Message = message;
}
