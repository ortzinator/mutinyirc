using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FlamingIRC;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.Tests;

/// <summary>
/// Dials a loopback TLS server that presents a self-signed certificate, so the normal check
/// fails every time. The trust lookup that the UI gives to <see cref="Server"/> decides what
/// happens next: with the network's verify switch on, the connection is refused; with it off,
/// the connection opens and says that it did not check the certificate.
/// </summary>
[TestFixture]
public class ServerCertificateTrustTests
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    private X509Certificate2 _certificate;
    private TcpListener _listener;
    private int _port;
    private Server _server;

    [OneTimeSetUp]
    public void CreateCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256,
                                             RSASignaturePadding.Pkcs1);
        using X509Certificate2 ephemeral =
            request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));
        // SslStream on Windows cannot use the key of an ephemeral certificate, so reload it from PFX.
        _certificate = new X509Certificate2(ephemeral.Export(X509ContentType.Pfx));
    }

    [OneTimeTearDown]
    public void DisposeCertificate() => _certificate.Dispose();

    [SetUp]
    public void StartServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _ = AcceptLoop();
    }

    [TearDown]
    public void StopServer()
    {
        _server?.Dispose();
        _server = null;
        _listener.Stop();
    }

    private async Task AcceptLoop()
    {
        while (true)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(); }
            catch (Exception) { return; } // The listener stopped.

            _ = Task.Run(async () =>
            {
                using (client)
                using (var ssl = new SslStream(client.GetStream()))
                {
                    try
                    {
                        await ssl.AuthenticateAsServerAsync(_certificate);
                        // Hold the link open until the client closes it.
                        var buffer = new byte[512];
                        while (await ssl.ReadAsync(buffer) > 0) { }
                    }
                    catch (Exception) { } // The client refused the certificate or hung up.
                }
            });
        }
    }

    private Server NewServer(Func<string, int, CertificateTrust> lookup)
    {
        _server = new Server(new ConnectionArgs("tester", "localhost", true) { Port = _port })
        {
            CertificateTrustLookup = lookup
        };
        return _server;
    }

    /// <summary>
    /// Completes with true when the connection opens, or with false when the connect fails.
    /// </summary>
    private static Task<bool> NextConnectResult(Server server)
    {
        var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Connected(object sender, EventArgs e) => result.TrySetResult(true);
        void Failed(object sender, ConnectFailedEventArgs e) => result.TrySetResult(false);
        server.Connected += Connected;
        server.ConnectFailed += Failed;
        result.Task.ContinueWith(_ =>
        {
            server.Connected -= Connected;
            server.ConnectFailed -= Failed;
        });
        return result.Task;
    }

    [Test]
    public async Task VerifySwitchOff_ConnectsAndSaysTheCertificateWasNotChecked()
    {
        Server server = NewServer((_, _) => new CertificateTrust { VerifyCertificates = false });

        Task<bool> result = NextConnectResult(server);
        server.Connect();

        Assert.That(await result.WaitAsync(Wait), Is.True,
            "a network with verification off accepts any certificate");
        Assert.That(server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.VerificationOff));
    }

    [Test]
    public async Task VerifySwitchOn_RefusesTheCertificate()
    {
        Server server = NewServer((_, _) => new CertificateTrust { VerifyCertificates = true });
        Task<ConnectFailedEventArgs> failure = NextConnectFailure(server);

        server.Connect();

        Assert.That((await failure.WaitAsync(Wait)).Reason, Is.EqualTo(ConnectError.CertificateRejected));
        Assert.That(server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.Trusted));
    }

    [Test]
    public async Task NoLookup_RefusesTheCertificate()
    {
        Server server = NewServer(null);
        Task<ConnectFailedEventArgs> failure = NextConnectFailure(server);

        server.Connect();

        Assert.That((await failure.WaitAsync(Wait)).Reason, Is.EqualTo(ConnectError.CertificateRejected),
            "a host with no trust settings is verified");
    }

    [Test]
    public async Task Lookup_GetsTheDialledEntryPoint()
    {
        string askedHost = null;
        int askedPort = 0;
        Server server = NewServer((host, port) =>
        {
            askedHost = host;
            askedPort = port;
            return CertificateTrust.Default;
        });
        Task<ConnectFailedEventArgs> failure = NextConnectFailure(server);

        server.Connect();
        await failure.WaitAsync(Wait);

        Assert.That((askedHost, askedPort), Is.EqualTo(("localhost", _port)),
            "a pin belongs to one entry point, so the lookup gets the host and the port");
    }

    /// <summary>
    /// The fingerprint of the test certificate, worked out apart from the code under test, in
    /// the display form but in lowercase. A pin in any case, with or without colons, matches.
    /// </summary>
    private string LowercaseDisplayFingerprint() =>
        string.Join(":", SHA256.HashData(_certificate.RawData).Select(b => b.ToString("x2")));

    [Test]
    public async Task APinThatMatchesTheCertificate_ConnectsThroughThePin()
    {
        Server server = NewServer((_, _) =>
            new CertificateTrust { PinnedFingerprint = LowercaseDisplayFingerprint() });

        Task<bool> result = NextConnectResult(server);
        server.Connect();

        Assert.That(await result.WaitAsync(Wait), Is.True, "a pin is a fallback to the normal check");
        Assert.That(server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.Pinned));
    }

    [Test]
    public async Task APinThatDoesNotMatchTheCertificate_RefusesIt()
    {
        Server server = NewServer((_, _) => new CertificateTrust { PinnedFingerprint = new string('A', 64) });
        Task<ConnectFailedEventArgs> failure = NextConnectFailure(server);

        server.Connect();

        Assert.That((await failure.WaitAsync(Wait)).Reason, Is.EqualTo(ConnectError.CertificateRejected));
        Assert.That(server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.Trusted));
    }

    [Test]
    public async Task APinThatMatches_SaysPinnedEvenWhenTheVerifySwitchIsOff()
    {
        Server server = NewServer((_, _) => new CertificateTrust
        {
            VerifyCertificates = false,
            PinnedFingerprint = LowercaseDisplayFingerprint()
        });

        Task<bool> result = NextConnectResult(server);
        server.Connect();

        Assert.That(await result.WaitAsync(Wait), Is.True);
        Assert.That(server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.Pinned),
            "the certificate matched the pin, so the connection did not go unchecked");
    }

    [Test]
    public async Task ASwitchChange_AppliesToTheNextConnectAttempt()
    {
        bool verify = true;
        Server server = NewServer((_, _) => new CertificateTrust { VerifyCertificates = verify });

        Task<ConnectFailedEventArgs> refused = NextConnectFailure(server);
        server.Connect();
        await refused.WaitAsync(Wait);

        verify = false;
        Task<bool> result = NextConnectResult(server);
        server.Connect();

        Assert.That(await result.WaitAsync(Wait), Is.True,
            "the lookup runs at each handshake, so the new value applies without a new Server");
    }

    private static Task<ConnectFailedEventArgs> NextConnectFailure(Server server)
    {
        var failed = new TaskCompletionSource<ConnectFailedEventArgs>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object sender, ConnectFailedEventArgs e)
        {
            server.ConnectFailed -= Handler;
            failed.TrySetResult(e);
        }
        server.ConnectFailed += Handler;
        return failed.Task;
    }
}
