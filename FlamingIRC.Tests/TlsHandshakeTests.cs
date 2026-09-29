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

    private static Task<ConnectError> NextConnectFailure(Connection connection)
    {
        var failed = new TaskCompletionSource<ConnectError>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object sender, ConnectFailedEventArgs e)
        {
            connection.ConnectFailed -= Handler;
            failed.TrySetResult(e.Reason);
        }
        connection.ConnectFailed += Handler;
        return failed.Task;
    }

    [Test]
    public async Task SelfSignedCertificate_FailsConnectWithAuthenticationFailed()
    {
        var connection = new Connection(Args(), false, false);
        var established = false;
        connection.ConnectionEstablished += (_, _) => established = true;

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.AuthenticationFailed),
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

        Assert.That(await second.WaitAsync(Wait), Is.EqualTo(ConnectError.AuthenticationFailed),
            "A redial must start from a new socket and reach the handshake again");
    }

    [Test]
    public async Task ValidationOverrideThatThrows_FailsConnectInsteadOfCrashing()
    {
        var connection = new ThrowingValidationConnection(Args());

        Task<ConnectError> failure = NextConnectFailure(connection);
        connection.Connect();

        Assert.That(await failure.WaitAsync(Wait), Is.EqualTo(ConnectError.AuthenticationFailed),
            "An exception from the validation override must count as a rejection");
    }

    private sealed class ThrowingValidationConnection(ConnectionArgs args) : Connection(args, false, false)
    {
        protected override bool OnCertificateValidationFailed(X509Certificate certificate, X509Chain chain,
                                                              SslPolicyErrors errors)
            => throw new InvalidOperationException("validation override failed");
    }
}
