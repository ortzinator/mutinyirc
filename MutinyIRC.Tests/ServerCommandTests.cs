using System;
using System.Collections.Generic;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using NUnit.Framework;

namespace MutinyIRC.Tests;

/// <summary>
/// Covers what <c>/server</c> dials. The command keeps the nick that the server in use has now,
/// because the configured nick lives in the UI layer, which commands cannot reach. The port
/// decides TLS through <see cref="IrcPorts"/>, and a bare host dials the TLS port.
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

    [Test]
    public void Execute_WithHost_DialsTheTlsPort()
    {
        new ServerCommand().Execute(_server, "irc.example.org");

        Assert.That(_server.LastArgs.Port, Is.EqualTo(6697));
        Assert.That(_server.LastArgs.Ssl, Is.True);
    }

    [TestCase("6697", true)]
    [TestCase("7000", true)]
    [TestCase("6667", false)]
    [TestCase("6665", false)]
    public void Execute_WithPort_ThePortDecidesTls(string port, bool tls)
    {
        new ServerCommand().Execute(_server, "irc.example.org", port);

        Assert.That(_server.LastArgs.Ssl, Is.EqualTo(tls));
    }

    [TestCase("abc")]
    [TestCase("0")]
    [TestCase("65536")]
    [TestCase("6697 extra")]
    public void Execute_WithBadPort_FailsAndKeepsTheServer(string port)
    {
        CommandResultInfo result = new ServerCommand().Execute(_server, "irc.example.org", port);

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        Assert.That(_server.LastArgs.Hostname, Is.EqualTo("old.example"));
    }

    [Test]
    public void Execute_WithUnknownSwitch_Fails()
    {
        CommandResultInfo result = new ServerCommand().Execute(_server, new[] { 'x' }, "irc.example.org");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        Assert.That(_server.LastArgs.Hostname, Is.EqualTo("old.example"));
    }

    [Test]
    public void Execute_WithNewWindowSwitchAndHost_DialsTheTlsPort()
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
            new ServerCommand().Execute(_server, new[] { 'n' }, "irc.example.org");

            Assert.That(created.Port, Is.EqualTo(6697));
            Assert.That(created.Connection.ConnectionData.Ssl, Is.True);
        }
        finally
        {
            ServerManager.Instance.ServerAdded -= OnAdded;
            created?.Dispose();
        }
    }

    private CommandResultInfo Dispatch(MessageContext context, params string[] args)
    {
        var manager = new PluginManager();
        Type type = typeof(ServerCommand);
        manager._commands.Add(type.FullName,
            new CommandInfo(type.Assembly.Location, type.FullName, "server", typeof(ICommand)));

        return manager.ExecuteCommand(new CommandExecutionInfo
        {
            Name = "server",
            Context = context,
            ParameterList = new List<object>(args),
        });
    }

    [Test]
    public void Dispatch_FromAChannelWindow_ChangesTheChannelsServer()
    {
        Dispatch(new Channel(_server, "#mutiny"), "irc.example.org", "6667");

        Assert.That(_server.LastArgs.Hostname, Is.EqualTo("irc.example.org"));
        Assert.That(_server.LastArgs.Port, Is.EqualTo(6667));
    }

    [Test]
    public void Dispatch_FromAQueryWindow_ChangesTheSessionsServer()
    {
        Dispatch(new PrivateMessageSession(_server, new User { Nick = "someone" }), "irc.example.org");

        Assert.That(_server.LastArgs.Hostname, Is.EqualTo("irc.example.org"));
    }
}
