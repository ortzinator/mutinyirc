using System;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Headless.NUnit;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the error block that the server window shows when the connection refuses a server
/// certificate: why the certificate was refused, and its fingerprint.
/// </summary>
[TestFixture]
public class ServerViewModelCertificateRefusalTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        SettingsSingleton.SetValue(null, new IrcSettingsManager(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                   System.IO.Path.GetRandomFileName() + ".json")));
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
    }

    /// <summary>
    /// Makes a view model over a faked TLS connection to irc.fake.com:6697. Set
    /// <paramref name="pinnedFingerprint" /> to give that entry point a pin.
    /// </summary>
    private void Dial(string? pinnedFingerprint = null)
    {
        var conn = A.Fake<IConnection>();
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("me", "irc.fake.com", true) { Port = 6697 });
        _server = new Server(conn)
        {
            CertificateTrustLookup = (host, port) =>
                host == "irc.fake.com" && port == 6697
                    ? new CertificateTrust { PinnedFingerprint = pinnedFingerprint }
                    : CertificateTrust.Default
        };
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    private void Refuse(X509Certificate2 certificate, SslPolicyErrors errors) =>
        _server.Connection.ConnectFailed += Raise.With(_server.Connection,
            new ConnectFailedEventArgs(ConnectError.CertificateRejected,
                                       new RejectedCertificate(certificate.RawData, errors)));

    // The connection has one entry point, so its refusal also stops the run. A second error
    // block ("Every server refused its certificate...") follows the refusal.
    private ErrorMessageViewModel TheErrorBlock() =>
        _vm.ChatLines.OfType<ErrorMessageViewModel>()
            .First(l => l.Message != ServerStrings.EveryEntryPointRefusedMessage);

    private static X509Certificate2 SelfSigned(string subject, DateTimeOffset notBefore, DateTimeOffset notAfter)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(notBefore, notAfter);
    }

    private static X509Certificate2 IssuedByCa(string subject, string dnsName)
    {
        using var caKey = RSA.Create(2048);
        var caRequest = new CertificateRequest("CN=Fake CA", caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using X509Certificate2 ca = caRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-10),
                                                               DateTimeOffset.UtcNow.AddYears(1));

        using var key = RSA.Create(2048);
        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName(dnsName);
        request.CertificateExtensions.Add(san.Build());
        return request.Create(ca, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30),
                              new byte[] { 1, 2, 3, 4 });
    }

    /// <summary>
    /// The fingerprint in display form, worked out apart from the code under test.
    /// </summary>
    private static string DisplayFingerprint(X509Certificate2 certificate) =>
        string.Join(":", SHA256.HashData(certificate.RawData).Select(b => b.ToString("X2")));

    [AvaloniaTest]
    public void SelfSignedRefusal_GivesTheSelfSignedReasonAndTheFullFingerprint()
    {
        using X509Certificate2 certificate = SelfSigned("CN=irc.fake.com",
                                                        DateTimeOffset.UtcNow.AddDays(-1),
                                                        DateTimeOffset.UtcNow.AddDays(30));
        Dial();

        Refuse(certificate, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.That(TheErrorBlock().Message, Is.EqualTo(
            "Refused irc.fake.com:6697: its certificate is not trusted, because it is self-signed, "
            + "so no trusted authority vouches for it.\n"
            + "SHA-256: " + DisplayFingerprint(certificate)));
    }

    [AvaloniaTest]
    public void NameMismatchOnACaIssuedCertificate_NamesTheCertificateSubject()
    {
        using X509Certificate2 certificate = IssuedByCa("CN=other.example.net", "other.example.net");
        Dial();

        Refuse(certificate, SslPolicyErrors.RemoteCertificateNameMismatch);

        Assert.That(TheErrorBlock().Message, Does.StartWith(
            "Refused irc.fake.com:6697: its certificate is not trusted, because it is issued for "
            + "other.example.net, not this host.\n"));
    }

    [AvaloniaTest]
    public void ExpiredCertificate_ShowsItsExpiryDate()
    {
        using X509Certificate2 certificate = SelfSigned("CN=irc.fake.com",
                                                        new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                                                        new DateTimeOffset(2020, 1, 15, 12, 0, 0, TimeSpan.Zero));
        Dial();

        Refuse(certificate, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.That(TheErrorBlock().Message, Does.StartWith(
            "Refused irc.fake.com:6697: its certificate is not trusted, because it is self-signed, "
            + "so no trusted authority vouches for it, and it expired on 2020-01-15.\n"),
            "A certificate that is both self-signed and expired gives both reasons");
    }

    [AvaloniaTest]
    public void ChainErrorWithNoKnownCause_SaysNoTrustedAuthorityVouchesForIt()
    {
        using X509Certificate2 certificate = IssuedByCa("CN=irc.fake.com", "irc.fake.com");
        Dial();

        Refuse(certificate, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.That(TheErrorBlock().Message, Does.StartWith(
            "Refused irc.fake.com:6697: its certificate is not trusted, because no trusted authority "
            + "vouches for it.\n"));
    }

    [AvaloniaTest]
    public void RefusalForAPinnedEntryPoint_WarnsThatTheCertificateChangedAndShowsBothFingerprints()
    {
        using X509Certificate2 certificate = SelfSigned("CN=irc.fake.com",
                                                        DateTimeOffset.UtcNow.AddDays(-1),
                                                        DateTimeOffset.UtcNow.AddDays(30));
        Dial(pinnedFingerprint: new string('A', 62) + "0F");

        Refuse(certificate, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.That(TheErrorBlock().Message, Is.EqualTo(
            "WARNING: the certificate of irc.fake.com:6697 has CHANGED since you pinned it. Someone may "
            + "be intercepting the connection, or the server got a new certificate.\n"
            + "Pinned: " + string.Join(":", Enumerable.Repeat("AA", 31)) + ":0F\n"
            + "Now: " + DisplayFingerprint(certificate)));
    }

    [AvaloniaTest]
    public void RefusalWithNoCertificate_StillShowsTheGenericMessage()
    {
        Dial();

        _server.Connection.ConnectFailed += Raise.With(_server.Connection,
            new ConnectFailedEventArgs(ConnectError.CertificateRejected));

        Assert.That(_vm.ChatLines.Select(l => l.Message),
            Does.Contain("Could not connect: the server's TLS certificate is not trusted."));
        Assert.That(_vm.ChatLines.OfType<ErrorMessageViewModel>().Select(l => l.Message),
            Is.EqualTo(new[] { ServerStrings.EveryEntryPointRefusedMessage }),
            "the only error block is the stop message, not a refusal block");
    }
}
