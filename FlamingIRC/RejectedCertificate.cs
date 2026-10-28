using System;
using System.Net.Security;
using System.Security.Cryptography;

namespace FlamingIRC;

/// <summary>
/// A server certificate that the TLS handshake rejected. Use it to show the user what the
/// connection refused, for example its fingerprint, so the user can decide to trust it.
/// </summary>
/// <remarks>
/// This is a copy of the certificate. The certificate that the handshake gives is disposed when
/// the handshake ends, but this copy stays valid.
/// </remarks>
/// <example>
/// <code>
/// connection.ConnectFailed += (_, e) =>
/// {
///     if (e.RejectedCertificate is { } certificate)
///         Console.WriteLine(certificate.GetFingerprint(HashAlgorithmName.SHA256));
/// };
/// </code>
/// </example>
public sealed class RejectedCertificate
{
    /// <param name="rawData">The certificate in DER encoding. The constructor copies it.</param>
    /// <param name="policyErrors">Why the normal check failed.</param>
    public RejectedCertificate(ReadOnlySpan<byte> rawData, SslPolicyErrors policyErrors)
    {
        RawData = rawData.ToArray();
        PolicyErrors = policyErrors;
    }

    /// <summary>
    /// The certificate in DER encoding.
    /// </summary>
    public ReadOnlyMemory<byte> RawData { get; }

    /// <summary>
    /// Why the normal check failed, for example an untrusted chain or a wrong host name.
    /// </summary>
    public SslPolicyErrors PolicyErrors { get; }

    /// <summary>
    /// Hash the DER encoding of the certificate.
    /// </summary>
    /// <param name="algorithm">The hash, for example <see cref="HashAlgorithmName.SHA256" />.</param>
    /// <returns>The hash as uppercase hex digits with no separators.</returns>
    /// <exception cref="CryptographicException">The platform does not support the hash.</exception>
    public string GetFingerprint(HashAlgorithmName algorithm)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(algorithm);
        hash.AppendData(RawData.Span);
        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
