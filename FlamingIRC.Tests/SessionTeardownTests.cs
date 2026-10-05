using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FlamingIRC.Tests;

/// <summary>
/// Dials a loopback server that ends each session as soon as it starts. The end of a live
/// session must leave the Connection ready to dial again, as the end of a failed connect does.
/// Before the fix, these paths shut the socket down but kept it, threw out of the I/O
/// callback, and a send that failed during registration left Connection.Connected true.
/// </summary>
[TestFixture]
public class SessionTeardownTests
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    private TcpListener _listener;
    private int _port;

    /// <summary>
    /// True: read the registration, then close, so the client reads end of stream.
    /// False: reset the connection at once, so the client's next send or read fails.
    /// </summary>
    private bool _readBeforeClose;

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
                {
                    if (_readBeforeClose)
                        await ReadUntilUser(client.GetStream());
                    else
                        // A zero linger time makes the close send RST, not FIN. The client then
                        // always sees a socket error, whether its send or its read comes first.
                        client.LingerState = new LingerOption(true, 0);
                }
            });
        }
    }

    private static async Task ReadUntilUser(NetworkStream stream)
    {
        var received = new StringBuilder();
        var buffer = new byte[512];
        while (!received.ToString().Contains("USER "))
        {
            int count = await stream.ReadAsync(buffer);
            if (count == 0)
                return;
            received.Append(Encoding.UTF8.GetString(buffer, 0, count));
        }
    }

    private Connection NewConnection() =>
        new(new ConnectionArgs("tester", "127.0.0.1", false) { Port = _port }, false, false);

    private static Task<DisconnectReason> NextConnectionLost(Connection connection)
    {
        var lost = new TaskCompletionSource<DisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(object sender, DisconnectEventArgs e)
        {
            connection.ConnectionLost -= Handler;
            lost.TrySetResult(e.Reason);
        }
        connection.ConnectionLost += Handler;
        return lost.Task;
    }

    [TestCase(true, DisconnectReason.RemoteHostClosedConnection, TestName = "RemoteClose_RedialOnSameConnection_ConnectsAgain")]
    [TestCase(false, DisconnectReason.SocketError, TestName = "SendReset_RedialOnSameConnection_ConnectsAgain")]
    public async Task SessionEnd_RedialOnSameConnection_ConnectsAgain(bool readBeforeClose, DisconnectReason expected)
    {
        _readBeforeClose = readBeforeClose;
        Connection connection = NewConnection();

        Task<DisconnectReason> first = NextConnectionLost(connection);
        connection.Connect();
        Assert.That(await first.WaitAsync(Wait), Is.EqualTo(expected));
        Assert.That(connection.Connected, Is.False, "The end of a session must leave Connected false");

        // The server ends this session too, so a good redial also ends as ConnectionLost.
        Task<DisconnectReason> second = NextConnectionLost(connection);
        Assert.DoesNotThrow(connection.Connect, "A redial must start from a new socket");
        Assert.That(await second.WaitAsync(Wait), Is.EqualTo(expected));
    }

    [Test]
    public async Task SendReset_EndsTheSessionOnce()
    {
        // Registration sends several lines. Each one after the first finds the session ended,
        // and must not report the loss again.
        _readBeforeClose = false;
        Connection connection = NewConnection();
        var losses = 0;
        connection.ConnectionLost += (_, _) => Interlocked.Increment(ref losses);

        Task<DisconnectReason> lost = NextConnectionLost(connection);
        connection.Connect();
        await lost.WaitAsync(Wait);
        await Task.Delay(200);

        Assert.That(Volatile.Read(ref losses), Is.EqualTo(1));
    }
}
