using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FlamingIRC.Tests;

/// <summary>
/// Drives a bare <see cref="TcpTextClient" /> against a loopback server. Each test tells the
/// server what to do with each accepted connection, and reads the client's hook calls in order.
/// These tests pin the session rules: each session starts clean, and only the current session
/// can end itself.
/// </summary>
[TestFixture]
public class TcpTextClientTests
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    private TcpListener _listener;
    private int _port;
    private CancellationTokenSource _stop;

    /// <summary>
    /// What the server does with the accepted connection. The argument is the zero-based
    /// number of the connection. The server closes the connection when the task completes.
    /// </summary>
    private Func<TcpClient, int, CancellationToken, Task> _serve;

    [SetUp]
    public void StartServer()
    {
        _stop = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _ = AcceptLoop();
    }

    [TearDown]
    public void StopServer()
    {
        _stop.Cancel();
        _listener.Stop();
        _stop.Dispose();
    }

    private async Task AcceptLoop()
    {
        for (var index = 0; ; index++)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(); }
            catch (Exception) { return; } // The listener stopped.

            int number = index;
            _ = Task.Run(async () =>
            {
                using (client)
                {
                    try
                    {
                        await _serve(client, number, _stop.Token);
                    }
                    catch (Exception e) when (e is OperationCanceledException or IOException or SocketException)
                    {
                        // The test ended, or the client closed the connection first. The test
                        // asserts on what the client reports, not on the server side.
                    }
                }
            });
        }
    }

    private static Task WriteAsync(TcpClient client, string text) =>
        WriteAsync(client, Encoding.UTF8.GetBytes(text));

    private static Task WriteAsync(TcpClient client, byte[] bytes) =>
        client.GetStream().WriteAsync(bytes).AsTask();

    private static Task HoldOpen(CancellationToken stop) => Task.Delay(Timeout.Infinite, stop);

    private static string Next(RecordingClient client)
    {
        Assert.That(client.Events.TryTake(out string e, Wait), Is.True, "The client must report the next event");
        return e;
    }

    [Test]
    public void PartLine_ThenRedial_NextSessionStartsWithACleanLine()
    {
        _serve = async (client, number, stop) =>
        {
            if (number == 0)
            {
                await WriteAsync(client, "LEFT"); // No line end. The close ends the session.
                return;
            }

            await WriteAsync(client, "WHOLE\r\n");
            await HoldOpen(stop);
        };
        var irc = new RecordingClient();

        irc.Dial(_port);
        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(Next(irc), Is.EqualTo("disconnect:RemoteHostClosedConnection"));

        irc.Dial(_port);
        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(Next(irc), Is.EqualTo("line:WHOLE"),
            "A part line from an ended session must not get into the next session");
    }

    [Test]
    public void CharacterSplitAcrossTwoReads_ArrivesWhole()
    {
        _serve = async (client, _, stop) =>
        {
            client.NoDelay = true;
            byte[] line = Encoding.UTF8.GetBytes("café\r\n");
            int split = line.Length - 3; // Between the two bytes of 'é'.
            await WriteAsync(client, line[..split]);
            // The pause makes the client read the two parts separately. Without it, TCP can
            // join them into one read, and the test still passes but no longer tests the split.
            await Task.Delay(200, stop);
            await WriteAsync(client, line[split..]);
            await HoldOpen(stop);
        };
        var irc = new RecordingClient();

        irc.Dial(_port);

        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(Next(irc), Is.EqualTo("line:café"));
    }

    [Test]
    public void DisconnectDuringHandshake_ThenRedial_OldHandshakeDoesNotEndTheNewSession()
    {
        var handshakeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _serve = async (client, number, stop) =>
        {
            if (number == 0)
            {
                // Accept, and never answer the TLS client hello.
                handshakeStarted.TrySetResult();
                await HoldOpen(stop);
                return;
            }

            await WriteAsync(client, "HELLO\r\n");
            await HoldOpen(stop);
        };
        var irc = new RecordingClient();

        irc.Dial(_port, ssl: true);
        Assert.That(handshakeStarted.Task.Wait(Wait), Is.True, "Precondition: the server accepted");
        irc.Disconnect(DisconnectReason.UserInitiated);
        Assert.That(Next(irc), Is.EqualTo("disconnect:UserInitiated"));

        irc.Dial(_port);
        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(Next(irc), Is.EqualTo("line:HELLO"));

        Assert.That(irc.Connected, Is.True, "The ended handshake must not end the session after it");
        Assert.That(irc.Events, Is.Empty, "The ended handshake must not report a connect failure");
    }

    [Test]
    public void Connect_WhileAnAttemptIsLive_Throws()
    {
        _serve = (_, _, stop) => HoldOpen(stop);
        var irc = new RecordingClient();

        irc.Dial(_port, ssl: true); // The handshake never completes, so the attempt stays live.

        Assert.Throws<InvalidOperationException>(() => irc.Dial(_port));
        irc.Disconnect(DisconnectReason.UserInitiated);
    }

    [Test]
    public void OnConnect_SeesConnectedTrue()
    {
        _serve = (_, _, stop) => HoldOpen(stop);
        var irc = new RecordingClient();

        irc.Dial(_port);

        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(irc.ConnectedInOnConnect, Is.True,
            "OnConnect sends the registration, so the client must already count as connected");
    }

    [Test]
    public void Disconnect_OnALiveSession_ReportsBeforeItReturns()
    {
        _serve = (_, _, stop) => HoldOpen(stop);
        var irc = new RecordingClient();
        irc.Dial(_port);
        Assert.That(Next(irc), Is.EqualTo("connect"));

        irc.Disconnect(DisconnectReason.UserInitiated);

        // TryTake with no wait: the event must be there already.
        Assert.That(irc.Events.TryTake(out string e), Is.True, "Server.Disconnect relies on this inline call");
        Assert.That(e, Is.EqualTo("disconnect:UserInitiated"));
        Assert.That(irc.Connected, Is.False);
    }

    [Test]
    public void Send_AfterTheSessionEnded_Throws()
    {
        _serve = (_, _, _) => Task.CompletedTask; // Close at once.
        var irc = new RecordingClient();
        irc.Dial(_port);
        Assert.That(Next(irc), Is.EqualTo("connect"));
        Assert.That(Next(irc), Does.StartWith("disconnect:"));

        Assert.Throws<InvalidOperationException>(() => irc.Send("PING x"));
    }

    /// <summary>
    /// Records each hook call as one event, in the order the calls occur.
    /// </summary>
    private sealed class RecordingClient : TcpTextClient
    {
        public BlockingCollection<string> Events { get; } = new();

        public bool ConnectedInOnConnect { get; private set; }

        public void Dial(int port, bool ssl = false) => Connect("127.0.0.1", port, ssl);

        protected override void OnConnect()
        {
            ConnectedInOnConnect = Connected;
            Events.Add("connect");
        }

        protected override void OnDisconnect(DisconnectReason reason, int? socketErrorCode) =>
            Events.Add($"disconnect:{reason}");

        protected override void OnConnectFailed(ConnectError error, int? socketErrorCode) =>
            Events.Add($"failed:{error}");

        protected override void OnReceiveLine(string line) => Events.Add($"line:{line}");
    }
}
