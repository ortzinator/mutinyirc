namespace MutinyIRC.Common;

/// <summary>
/// The trust settings that apply to one entry point when its server certificate fails the
/// normal check. The host gives them to <see cref="Server.CertificateTrustLookup"/>.
/// </summary>
/// <remarks>
/// Each setting is an init property with a safe default, so a new setting can come later
/// without a change to the code that makes one.
/// </remarks>
/// <example>
/// <code>
/// server.CertificateTrustLookup = host => new CertificateTrust { VerifyCertificates = false };
/// </code>
/// </example>
public sealed record CertificateTrust
{
    /// <summary>
    /// The settings for a host that no saved network lists: verify every certificate.
    /// </summary>
    public static CertificateTrust Default { get; } = new();

    /// <summary>
    /// The network's "Verify TLS certificates" switch. When it is false, the connection
    /// accepts any certificate. It is true by default.
    /// </summary>
    public bool VerifyCertificates { get; init; } = true;
}
