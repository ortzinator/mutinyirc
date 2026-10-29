using System;

namespace MutinyIRC.Common;

/// <summary>
/// The trust settings that apply to one entry point when its server certificate fails the
/// normal check: the network's verify switch and the entry point's pin. The host gives them to
/// <see cref="Server.CertificateTrustLookup"/>.
/// </summary>
/// <remarks>
/// Each setting is an init property with a safe default, so a new setting can come later
/// without a change to the code that makes one.
/// </remarks>
/// <example>
/// <code>
/// server.CertificateTrustLookup = (host, port) => new CertificateTrust { VerifyCertificates = false };
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

    /// <summary>
    /// The SHA-256 fingerprint of the certificate that is pinned on the entry point, as hex
    /// digits with or without colons, in either case. Null, the default, means no pin. A
    /// certificate that fails the normal check but matches the pin is accepted.
    /// </summary>
    public string PinnedFingerprint { get; init; }

    /// <summary>
    /// Whether <paramref name="fingerprint"/> is the pinned fingerprint. It ignores colons and
    /// case. It is false when there is no pin.
    /// </summary>
    public bool IsPinned(string fingerprint) =>
        PinnedFingerprint != null && fingerprint != null
        && string.Equals(Normalize(PinnedFingerprint), Normalize(fingerprint),
                         StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string fingerprint) => fingerprint.Replace(":", string.Empty).Trim();
}
