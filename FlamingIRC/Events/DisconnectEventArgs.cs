using System;

namespace FlamingIRC;

/// <summary>
/// Event data raised when the connection to the IRC server is closed,
/// whether by the user, the remote host, or a socket failure.
/// </summary>
public class DisconnectEventArgs : EventArgs
{
    /// <summary>
    /// Why the connection was closed.
    /// </summary>
    public DisconnectReason Reason;

    /// <summary>
    /// The underlying socket error code when <see cref="Reason"/> is
    /// <see cref="DisconnectReason.SocketError"/>; zero when the
    /// disconnect did not originate from a socket failure.
    /// </summary>
    public int SocketErrorCode;

    /// <summary>
    /// Initializes a new instance with both a reason and a socket error code.
    /// Use this overload when the disconnect was triggered by a socket failure.
    /// </summary>
    /// <param name="reason">Why the connection was closed.</param>
    /// <param name="socketErrorCode">The underlying socket error code.</param>
    public DisconnectEventArgs(DisconnectReason reason, int socketErrorCode)
    {
        Reason = reason;
        SocketErrorCode = socketErrorCode;
    }

    /// <summary>
    /// Initializes a new instance for a disconnect that has no associated
    /// socket error code (e.g. <see cref="DisconnectReason.UserInitiated"/>
    /// or <see cref="DisconnectReason.RemoteHostClosedConnection"/>).
    /// </summary>
    /// <param name="reason">Why the connection was closed.</param>
    public DisconnectEventArgs(DisconnectReason reason) => Reason = reason;
}
