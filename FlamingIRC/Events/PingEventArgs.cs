using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnPing"/>.
/// </summary>
public sealed class PingEventArgs : EventArgs
{
    /// <summary>
    /// The text that the server wants back in the PONG reply.
    /// </summary>
    public string Message { get; }

    public PingEventArgs(string message) => Message = message;
}
