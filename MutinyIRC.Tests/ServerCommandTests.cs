using System.Collections.Generic;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.Tests;

/// <summary>
/// Covers the nick that <c>/server</c> connects with. The command keeps the nick that the
/// server in use has now, because the configured nick lives in the UI layer, which commands
/// cannot reach.
/// </summary>
[TestFixture]
public class ServerCommandTests
{
    /// <summary>
    /// A <see cref="Server"/> whose connections are fakes, so a server change opens no socket.
    /// It keeps the arguments of the newest connection.
    /// </summary>
    private sealed class RecordingServer : Server
    {
        public ConnectionArgs LastArgs { get; private set; }

        public RecordingServer(ConnectionArgs args) : base(new List<ConnectionArgs> { args }) { }

        protected override IConnection CreateConnection(ConnectionArgs args)
        {
            var conn = A.Fake<IConnection>();
            // A real Listener: UnhookEvents detaches from Connection.Listener, which would
            // throw against a fake's default null.
            A.CallTo(() => conn.Listener).Returns(new Listener());
            A.CallTo(() => conn.ConnectionData).Returns(args);

            LastArgs = args;
            return conn;
        }
    }

    private RecordingServer _server;

    [SetUp]
    public void SetUp() =>
        _server = new RecordingServer(new ConnectionArgs("Sonja", "old.example", false));

    [TearDown]
    public void TearDown() => _server.Dispose();

    [Test]
    public void Execute_WithHost_KeepsTheCurrentNick()
    {
        new ServerCommand().Execute(_server, "irc.example.org");

        Assert.That(_server.LastArgs.Hostname, Is.EqualTo("irc.example.org"));
        Assert.That(_server.LastArgs.Nick, Is.EqualTo("Sonja"));
    }

    [Test]
    public void Execute_WithHostAndPort_KeepsTheCurrentNick()
    {
        new ServerCommand().Execute(_server, "irc.example.org", "6697");

        Assert.That(_server.LastArgs.Port, Is.EqualTo(6697));
        Assert.That(_server.LastArgs.Nick, Is.EqualTo("Sonja"));
    }

    [Test]
    public void Execute_WithNewWindowSwitch_CreatesAServerWithTheCurrentNick()
    {
        Server created = null;
        void OnAdded(object sender, ServerEventArgs e)
        {
            created = e.Server;
            // Cancel the dial, so the new server opens no socket.
            created.Connecting += (_, c) => c.Cancel = true;
        }
        ServerManager.Instance.ServerAdded += OnAdded;

        try
        {
            new ServerCommand().Execute(_server, new[] { 'n' }, "irc.example.org", "6697");

            Assert.That(created, Is.Not.Null);
            Assert.That(created.UserNick, Is.EqualTo("Sonja"));
        }
        finally
        {
            ServerManager.Instance.ServerAdded -= OnAdded;
            created?.Dispose();
        }
    }
}
