using System;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace FlamingIRC.Tests;

[TestFixture]
public class RejectedCertificateTests
{
    private static readonly byte[] Abc = Encoding.ASCII.GetBytes("abc");

    [Test]
    public void GetFingerprint_IsUppercaseHexWithNoSeparators()
    {
        var certificate = new RejectedCertificate(Abc, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.That(certificate.GetFingerprint(HashAlgorithmName.SHA256),
            Is.EqualTo("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD"));
    }

    [Test]
    public void RawData_IsACopy()
    {
        var bytes = (byte[])Abc.Clone();
        var certificate = new RejectedCertificate(bytes, SslPolicyErrors.RemoteCertificateChainErrors);

        bytes[0] = 0;

        Assert.That(certificate.RawData.ToArray(), Is.EqualTo(Abc),
            "A change to the caller's array must not change the report");
    }

    [Test]
    public void ConnectFailedEventArgs_WithCertificate_CarriesIt()
    {
        var certificate = new RejectedCertificate(Abc, SslPolicyErrors.RemoteCertificateNameMismatch);

        var args = new ConnectFailedEventArgs(ConnectError.CertificateRejected, certificate);

        Assert.That(args.Reason, Is.EqualTo(ConnectError.CertificateRejected));
        Assert.That(args.RejectedCertificate, Is.SameAs(certificate));
    }

    [Test]
    public void ConnectFailedEventArgs_WithCertificateForAnotherReason_Throws()
    {
        var certificate = new RejectedCertificate(Abc, SslPolicyErrors.RemoteCertificateNameMismatch);

        Assert.Throws<ArgumentException>(() =>
            _ = new ConnectFailedEventArgs(ConnectError.AuthenticationFailed, certificate));
    }

    [Test]
    public void ConnectFailedEventArgs_WithoutCertificate_HasNone()
    {
        Assert.That(new ConnectFailedEventArgs(ConnectError.CertificateRejected).RejectedCertificate, Is.Null);
        Assert.That(new ConnectFailedEventArgs(ConnectError.SocketError, 10061).RejectedCertificate, Is.Null);
    }
}
