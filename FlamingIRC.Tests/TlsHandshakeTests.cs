using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FlamingIRC.Tests;

/// <summary>
/// Dials a loopback TLS server that presents a self-signed certificate. A failed certificate
/// check must end as a ConnectFailed event. Before the fix, it ended the process: the
/// validation override threw on a thread-pool thread, and a redial on the same Connection
/// found the old socket still connected.
/// </summary>
[TestFixture]
public class TlsHandshakeTests
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    private X509Certificate2 _certificate;
    private TcpListener _listener;
    private int _port;

    [OneTimeSetUp]
    public void CreateCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using X509Certificate2 ephemeral = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));
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
    public void StopServer() => _listener.Stop();

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
                    // The client rejects the certificate, so the server side of the handshake fails.
                    try { await ssl.AuthenticateAsServerAsync(_certificate); }
                    catch (Exception) { }
                }
            });
        }
    }

    private ConnectionArgs Args() => new ConnectionArgs("tester", "localhost", true) { Port = _port };

    private static async Task<ConnectError> NextConnectFailure(Connection connection) =>
        (await NextConnectFailedArgs(connection)).Reason;

    private static Task<ConnectFailedEventArgs> NextConnectFailedArgs(Connection connection)
    {
        var failed = new TaskCompletionSource<ConnectFailedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object sender, ConnectFailedEventArgs e)
        {
            connection.ConnectFailed -= Handler;
            failed.TrySetResult(e);
        }
        connection.ConnectFailed += Handler;
        return failed.Task;
    }

    /// <summary>
    /// Completes with true when the connection opens, or with false when the connect fails.
    /// </summary>
    private static Task<bool> NextConnectResult(Connection connection)
    {
        var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.ConnectionEstablished += (_, _) => result.TrySetResult(true);
        connection.ConnectFailed += (_, _) => result.TrySetResult(false);
        return result.Task;
    }

    [Test]
    public async Task SelfSignedCertificate_FailsConnectWithCertificateRejected()
    {
        var connection = new Connection(Args(), false, false);
        var established = false;
        connection.ConnectionEstablished += (_, _) => established = true;

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.CertificateRejected),
            "An untrusted certificate must fail the connect, not end the process");
        Assert.That(established, Is.False, "An untrusted certificate must not be accepted");
    }

    [Test]
    public async Task SelfSignedCertificate_RedialOnSameConnection_FailsCleanlyAgain()
    {
        // Server reuses one Connection when it retries a single-host network.
        var connection = new Connection(Args(), false, false);

        Task<ConnectError> first = NextConnectFailure(connection);
        connection.Connect();
        await first.WaitAsync(Wait);

        Task<ConnectError> second = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await second.WaitAsync(Wait), Is.EqualTo(ConnectError.CertificateRejected),
            "A redial must start from a new socket and reach the handshake again");
    }

    [Test]
    public async Task ValidationOverrideThatThrows_FailsConnectInsteadOfCrashing()
    {
        var connection = new ThrowingValidationConnection(Args());

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.CertificateRejected),
            "An exception from the validation override must count as a rejection");
    }

    [Test]
    public async Task CallbackThatAccepts_Connects()
    {
        var connection = new Connection(Args(), false, false)
        {
            CertificateValidationFailedCallback = (_, _, _) => true
        };

        Task<bool> result = NextConnectResult(connection);
        connection.Connect();

        Assert.That(await result.WaitAsync(Wait), Is.True,
            "A callback that returns true must accept the certificate");
    }

    [Test]
    public async Task CallbackThatRejects_FailsConnectWithCertificateRejected()
    {
        var connection = new Connection(Args(), false, false)
        {
            CertificateValidationFailedCallback = (_, _, _) => false
        };

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.CertificateRejected));
    }

    [Test]
    public async Task CallbackThatThrows_FailsConnectInsteadOfCrashing()
    {
        var connection = new Connection(Args(), false, false)
        {
            CertificateValidationFailedCallback = (_, _, _) => throw new InvalidOperationException("callback failed")
        };

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.CertificateRejected),
            "An exception from the callback must count as a rejection");
    }

    [Test]
    public void Callback_IsNotCalledForACertificateThatPassesTheNormalCheck()
    {
        var called = false;
        var connection = new Connection(Args(), false, false)
        {
            CertificateValidationFailedCallback = (_, _, _) => called = true
        };

        bool accepted = connection.OnCertificateValidate(_certificate, null, SslPolicyErrors.None);

        Assert.That(accepted, Is.True);
        Assert.That(called, Is.False, "The callback is only for a certificate that fails the normal check");
    }

    [Test]
    public async Task CertificateRejected_CarriesTheServerCertificate()
    {
        var connection = new Connection(Args(), false, false);

        Task<ConnectFailedEventArgs> failure = NextConnectFailedArgs(connection);
        connection.Connect();
        ConnectFailedEventArgs args = await failure.WaitAsync(Wait);

        Assert.That(args.Reason, Is.EqualTo(ConnectError.CertificateRejected));
        Assert.That(args.RejectedCertificate, Is.Not.Null);
        Assert.That(args.RejectedCertificate.RawData.ToArray(), Is.EqualTo(_certificate.RawData));
        Assert.That(args.RejectedCertificate.GetFingerprint(HashAlgorithmName.SHA256),
            Is.EqualTo(_certificate.GetCertHashString(HashAlgorithmName.SHA256)));
        Assert.That(args.RejectedCertificate.PolicyErrors, Is.Not.EqualTo(SslPolicyErrors.None));
    }

    [Test]
    public async Task SocketFailure_CarriesNoCertificate()
    {
        _listener.Stop();
        var connection = new Connection(Args(), false, false);

        Task<ConnectFailedEventArgs> failure = NextConnectFailedArgs(connection);
        connection.Connect();
        ConnectFailedEventArgs args = await failure.WaitAsync(Wait);

        Assert.That(args.Reason, Is.Not.EqualTo(ConnectError.CertificateRejected));
        Assert.That(args.RejectedCertificate, Is.Null);
    }

    private sealed class ThrowingValidationConnection(ConnectionArgs args) : Connection(args, false, false)
    {
        protected override bool OnCertificateValidationFailed(X509Certificate certificate, X509Chain chain,
                                                              SslPolicyErrors errors)
            => throw new InvalidOperationException("validation override failed");
    }
}
