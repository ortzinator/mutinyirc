namespace MutinyIRC.Common;

/// <summary>
/// Why the current connection accepted the server certificate. See
/// <see cref="Server.CertificateAcceptance"/>.
/// </summary>
public enum CertificateAcceptance
{
    /// <summary>
    /// The certificate passed the normal check, or the connection has no TLS. This is the
    /// usual case, and it needs no warning.
    /// </summary>
    Trusted,

    /// <summary>
    /// The certificate failed the normal check, and the connection accepted it only because
    /// the network's verify switch is off.
    /// </summary>
    VerificationOff
}
