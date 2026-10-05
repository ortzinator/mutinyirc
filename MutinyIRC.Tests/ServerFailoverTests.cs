using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.Tests;

/// <summary>
/// Covers the walk over a network's entry points. A network dials its hosts in the order the
/// user put them, moving on at once when one does not answer, and waits only when a whole pass
/// has failed. That is what makes one backoff round mean one pass rather than one host.
/// </summary>
[TestFixture]
public class ServerFailoverTests
{
    /// <summary>
    /// A <see cref="Server"/> whose connections are fakes, so the walk can be driven without
    /// opening a socket. It records every dial and keeps the connection currently in use, which
    /// is the one a failure has to be raised on.
    /// </summary>
    private sealed class WalkingServer : Server
    {
        public List<string> Dialled { get; } = new();

        public IConnection Current { get; private set; } = null!;

        public WalkingServer(IReadOnlyList<ConnectionArgs> candidates) : base(candidates) { }

        protected override IConnection CreateConnection(ConnectionArgs args)
        {
            var conn = A.Fake<IConnection>();
            A.CallTo(() => conn.Connected).Returns(false);
            // A real Listener: UnhookEvents detaches from Connection.Listener, which would
            // throw against a fake's default null.
            A.CallTo(() => conn.Listener).Returns(new Listener());
            A.CallTo(() => conn.ConnectionData).Returns(args);
            A.CallTo(() => conn.TryConnect())
                .Invokes(() => Dialled.Add($"{args.Hostname}:{args.Port}"))
                .Returns(true);

            Current = conn;
            return conn;
        }

        /// <summary>Tells the connection in use that the host did not answer.</summary>
        public void FailCurrentDial() =>
            Current.ConnectFailed += Raise.With(this,
                new ConnectFailedEventArgs(ConnectError.ConnectionRefused));

        /// <summary>Feeds the welcome reply, which is what ends a run of tries.</summary>
        public void Register() =>
            Current.Listener.Parse(":irc.example 001 nick :Welcome to the network");
    }

    private static ConnectionArgs Candidate(string host, int port) =>
        new("nick", host, false) { Port = port };

    private WalkingServer _server = null!;
    private List<int> _reconnectAttempts = null!;

    private WalkingServer Build(params ConnectionArgs[] candidates)
    {
        _server = new WalkingServer(candidates);
        _reconnectAttempts = new List<int>();
        _server.Reconnecting += (_, e) => _reconnectAttempts.Add(e.Attempt);
        return _server;
    }

    [TearDown]
    public void TearDown() => _server?.Dispose();

    // ── One entry point behaves exactly as before ────────────────────────────

    [Test]
    public void OneEntryPoint_AFailedDial_SchedulesAReconnect()
    {
        Build(Candidate("irc.libera.chat", 6667));

        _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void OneEntryPoint_AFailedDial_StaysOnTheSameHost()
    {
        Build(Candidate("irc.libera.chat", 6667));

        _server.FailCurrentDial();

        Assert.That(_server.Url, Is.EqualTo("irc.libera.chat"));
    }

    [Test]
    public void OneEntryPoint_TwoFailures_CountUp()
    {
        Build(Candidate("irc.libera.chat", 6667));

        _server.FailCurrentDial();
        _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1, 2 }));
    }

    // ── The walk ─────────────────────────────────────────────────────────────

    [Test]
    public void AFailedDial_MovesToTheNextEntryPoint()
    {
        Build(Candidate("first.example", 6697), Candidate("second.example", 6667));

        _server.FailCurrentDial();

        Assert.That(_server.Url, Is.EqualTo("second.example"));
    }

    [Test]
    public void AFailedDial_DialsTheNextHostAtOnce()
    {
        Build(Candidate("first.example", 6697), Candidate("second.example", 6667));

        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "second.example:6667" }));
            Assert.That(_reconnectAttempts, Is.Empty,
                "a host that is down is not a reason to wait: the next host is tried now");
        });
    }

    [Test]
    public void AFailedDial_CarriesTheEntryPointsOwnPort()
    {
        Build(Candidate("first.example", 6697), Candidate("second.example", 7000));

        _server.FailCurrentDial();

        Assert.That(_server.Port, Is.EqualTo(7000));
    }

    [Test]
    public void AFailedDial_KeepsTheNickInUse()
    {
        // Identity belongs to the network, so moving between its hosts must not change it.
        Build(Candidate("first.example", 6697), Candidate("second.example", 6667));

        _server.FailCurrentDial();

        Assert.That(_server.Current.ConnectionData.Nick, Is.EqualTo("nick"));
    }

    [Test]
    public void TheWalk_TriesEveryHostInOrder()
    {
        Build(Candidate("a.example", 6697),
              Candidate("b.example", 6697),
              Candidate("c.example", 6697));

        _server.FailCurrentDial();
        _server.FailCurrentDial();

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697", "c.example:6697" }));
    }

    // ── Running off the end ──────────────────────────────────────────────────

    [Test]
    public void APassThatFailsEverywhere_BacksOffOnce()
    {
        Build(Candidate("a.example", 6697),
              Candidate("b.example", 6697),
              Candidate("c.example", 6697));

        _server.FailCurrentDial();
        _server.FailCurrentDial();
        _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }),
            "one backoff round is one full pass over the hosts, not one per host");
    }

    [Test]
    public void APassThatFailsEverywhere_ReturnsToTheFirstHost()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        _server.FailCurrentDial();
        _server.FailCurrentDial();

        Assert.That(_server.Url, Is.EqualTo("a.example"));
    }

    [Test]
    public void APassThatFailsEverywhere_DoesNotDialAgainItself()
    {
        // The scheduled reconnect dials; the wrap only puts the connection back on the
        // first host so the timer has somewhere to go.
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        _server.FailCurrentDial();
        _server.FailCurrentDial();

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697" }));
    }

    [Test]
    public void TwoFailedPasses_CountTwoTries()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        for (int i = 0; i < 4; i++)
            _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1, 2 }));
    }

    // ── Registration ends the run ────────────────────────────────────────────

    [Test]
    public void Registering_ResetsTheTryCount()
    {
        Build(Candidate("a.example", 6697));

        _server.FailCurrentDial();
        _server.Register();
        _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1, 1 }),
            "back online ends the run, so the next drop waits the base delay again");
    }

    [Test]
    public void Registering_LeavesTheWalkOnTheHostThatAnswered()
    {
        // The index names the live connection, so a later drop retries the host you were
        // on before the walk moves off it.
        Build(Candidate("a.example", 6697),
              Candidate("b.example", 6697),
              Candidate("c.example", 6697));

        _server.FailCurrentDial();
        _server.Register();
        _server.FailCurrentDial();

        Assert.That(_server.Url, Is.EqualTo("c.example"));
    }

    // ── The user taking over ─────────────────────────────────────────────────

    [Test]
    public void ChangeServer_ReplacesTheEntryPoints()
    {
        // /server names a host, so failing over to the old network's hosts would dial a
        // network the user has left.
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        _server.ChangeServer(Candidate("chosen.example", 6667));
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Url, Is.EqualTo("chosen.example"));
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
        });
    }

    [Test]
    public void ChangeServer_DuringAConnectAttempt_StopsTheOldAttempt()
    {
        // The old connection is not connected yet, but its attempt is in flight. Unhooked and
        // left alone, it would connect with nothing to hear it.
        Build(Candidate("a.example", 6667));
        IConnection old = _server.Current;

        _server.ChangeServer(Candidate("chosen.example", 6667));

        A.CallTo(() => old.Disconnect(A<string>._)).MustHaveHappened();
    }

    [Test]
    public async Task TwoServerChangesWithinASecond_DialOnlyTheLastHost()
    {
        // Each change holds its dial back for a second. The second change must cancel the
        // first dial, so the new connection is not dialled twice.
        Build(Candidate("a.example", 6667));

        _server.ChangeServer(Candidate("first.example", 6667));
        _server.Connect();
        _server.ChangeServer(Candidate("second.example", 6667));
        _server.Connect();
        await Task.Delay(TimeSpan.FromSeconds(1.5));

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "second.example:6667" }));
    }

    [Test]
    public async Task Disconnect_CancelsADialHeldBackAfterAServerChange()
    {
        Build(Candidate("a.example", 6667));

        _server.ChangeServer(Candidate("chosen.example", 6667));
        _server.Connect();
        _server.Disconnect();
        await Task.Delay(TimeSpan.FromSeconds(1.5));

        Assert.That(_server.Dialled, Is.Empty);
    }

    // ── Guards ───────────────────────────────────────────────────────────────

    [Test]
    public void Constructor_NoEntryPoints_Throws()
    {
        Assert.That(() => new Server(Array.Empty<ConnectionArgs>()),
            Throws.ArgumentException);
    }
}
