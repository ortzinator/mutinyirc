using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI;

/// <summary>
/// Writes the text of the error block that the server window shows when the connection refuses
/// a server certificate.
/// </summary>
internal static class CertificateRefusal
{
    /// <summary>
    /// Make the refusal text for the entry point <paramref name="host" />:<paramref name="port" />.
    /// </summary>
    /// <param name="pinnedFingerprint">
    /// The fingerprint that is pinned on this entry point, or null when it has no pin. When it has
    /// a pin, the text warns that the certificate changed.
    /// </param>
    public static string Describe(RejectedCertificate certificate, string host, int port,
                                  string? pinnedFingerprint)
    {
        string entryPoint = $"{host}:{port}";
        string fingerprint = FormatFingerprint(certificate.GetFingerprint(HashAlgorithmName.SHA256));

        if (pinnedFingerprint is not null)
        {
            return string.Join("\n",
                ServerStrings.CertificateChangedMessage.With(entryPoint),
                ServerStrings.CertificatePinnedLine.With(FormatFingerprint(pinnedFingerprint)),
                ServerStrings.CertificateNowLine.With(fingerprint));
        }

        return string.Join("\n",
            ServerStrings.CertificateRefusedMessage.With(entryPoint, Reason(certificate)),
            ServerStrings.CertificateFingerprintLine.With(fingerprint));
    }

    /// <summary>
    /// Show a fingerprint as colon-separated pairs of uppercase hex digits, for example
    /// "AB:CD:01". The input can have colons or not, in either case.
    /// </summary>
    public static string FormatFingerprint(string fingerprint)
    {
        string hex = fingerprint.Replace(":", string.Empty).ToUpperInvariant();
        var pairs = new List<string>(hex.Length / 2);
        for (int i = 0; i + 1 < hex.Length; i += 2)
            pairs.Add(hex.Substring(i, 2));
        return string.Join(":", pairs);
    }

    /// <summary>
    /// Say why the normal check refused the certificate. When more than one reason applies, join
    /// them with ", and ".
    /// </summary>
    private static string Reason(RejectedCertificate rejected)
    {
        // The DER copy stays valid after the handshake, so load the certificate again from it.
        using var certificate = new X509Certificate2(rejected.RawData.ToArray());
        var reasons = new List<string>();

        if (rejected.PolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
        {
            reasons.Add(ServerStrings.CertificateReasonNameMismatch.With(
                certificate.GetNameInfo(X509NameType.DnsName, false)));
        }

        bool selfSigned = certificate.SubjectName.RawData.AsSpan()
            .SequenceEqual(certificate.IssuerName.RawData);
        if (selfSigned)
            reasons.Add(ServerStrings.CertificateReasonSelfSigned);

        bool expired = certificate.NotAfter < DateTime.Now;
        if (expired)
            reasons.Add(ServerStrings.CertificateReasonExpired.With(certificate.NotAfter.ToString("yyyy-MM-dd")));

        // A chain error that the reasons above do not explain, for example an unknown authority.
        bool otherChainError = rejected.PolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors)
                               && !selfSigned && !expired;
        if (otherChainError || reasons.Count == 0)
            reasons.Add(ServerStrings.CertificateReasonUntrusted);

        return string.Join(ServerStrings.CertificateReasonSeparator, reasons);
    }
}
