using System;

namespace FlamingIRC;

/// <summary>
/// Event data raised when an attempt to connect to the IRC server fails
/// before the connection is established (e.g. refused, authentication
/// failed, or socket error during connect).
/// </summary>
public class ConnectFailedEventArgs : EventArgs
{
    /// <summary>
    /// Why the connection attempt failed.
    /// </summary>
    public ConnectError Reason;

    /// <summary>
    /// The underlying socket error code when <see cref="Reason"/> is
    /// <see cref="ConnectError.SocketError"/>; zero when the failure
    /// did not originate from a socket error.
    /// </summary>
    public int SocketErrorCode;

    /// <summary>
    /// Initializes a new instance with both a reason and a socket error code.
    /// Use this overload when the failure was triggered by a socket error.
    /// </summary>
    /// <param name="reason">Why the connection attempt failed.</param>
    /// <param name="socketErrorCode">The underlying socket error code.</param>
    public ConnectFailedEventArgs(ConnectError reason, int socketErrorCode)
    {
        Reason = reason;
        SocketErrorCode = socketErrorCode;
    }

    /// <summary>
    /// Initializes a new instance for a failure that has no associated
    /// socket error code (e.g. <see cref="ConnectError.AuthenticationFailed"/>).
    /// </summary>
    /// <param name="reason">Why the connection attempt failed.</param>
    public ConnectFailedEventArgs(ConnectError reason) => Reason = reason;
}
