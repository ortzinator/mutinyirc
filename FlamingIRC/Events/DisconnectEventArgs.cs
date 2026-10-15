using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Connection.ConnectionLost"/>. The connection to the IRC server
/// closed. The user, the remote host or a socket failure closed it.
/// </summary>
public sealed class DisconnectEventArgs : EventArgs
{
    /// <summary>
    /// The reason that the connection closed.
    /// </summary>
    public DisconnectReason Reason { get; }

    /// <summary>
    /// The socket error code when <see cref="Reason"/> is <see cref="DisconnectReason.SocketError"/>.
    /// For all other reasons, it is zero.
    /// </summary>
    public int SocketErrorCode { get; }

    /// <summary>
    /// Use this constructor when a socket failure caused the disconnect.
    /// </summary>
    public DisconnectEventArgs(DisconnectReason reason, int socketErrorCode)
    {
        Reason = reason;
        SocketErrorCode = socketErrorCode;
    }

    /// <summary>
    /// Use this constructor when the disconnect has no socket error code, for example
    /// <see cref="DisconnectReason.UserInitiated"/> or
    /// <see cref="DisconnectReason.RemoteHostClosedConnection"/>.
    /// </summary>
    public DisconnectEventArgs(DisconnectReason reason) => Reason = reason;
}
