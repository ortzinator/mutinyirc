using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Connection.ConnectFailed"/>. The attempt to connect to the IRC server
/// failed before the connection opened. For example, the server refused the connection,
/// authentication failed, or a socket error occurred.
/// </summary>
public sealed class ConnectFailedEventArgs : EventArgs
{
    /// <summary>
    /// The reason that the connection attempt failed.
    /// </summary>
    public ConnectError Reason { get; }

    /// <summary>
    /// The socket error code when <see cref="Reason"/> is <see cref="ConnectError.SocketError"/>.
    /// For all other reasons, it is zero.
    /// </summary>
    public int SocketErrorCode { get; }

    /// <summary>
    /// The server certificate that the connection refused, when <see cref="Reason"/> is
    /// <see cref="ConnectError.CertificateRejected"/>. It is null for all other reasons, and also
    /// when the server sent no certificate.
    /// </summary>
    public RejectedCertificate RejectedCertificate { get; }

    /// <summary>
    /// Use this constructor when a socket error caused the failure.
    /// </summary>
    public ConnectFailedEventArgs(ConnectError reason, int socketErrorCode)
    {
        Reason = reason;
        SocketErrorCode = socketErrorCode;
    }

    /// <summary>
    /// Use this constructor when the failure has no socket error code, for example
    /// <see cref="ConnectError.AuthenticationFailed"/>.
    /// </summary>
    public ConnectFailedEventArgs(ConnectError reason) => Reason = reason;

    /// <summary>
    /// Use this constructor when the connection refused the server certificate.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="rejectedCertificate"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="reason"/> is not <see cref="ConnectError.CertificateRejected"/>.
    /// </exception>
    public ConnectFailedEventArgs(ConnectError reason, RejectedCertificate rejectedCertificate)
    {
        ArgumentNullException.ThrowIfNull(rejectedCertificate);
        if (reason != ConnectError.CertificateRejected)
            throw new ArgumentException(
                $"Only a {nameof(ConnectError.CertificateRejected)} failure has a certificate.", nameof(reason));

        Reason = reason;
        RejectedCertificate = rejectedCertificate;
    }
}
