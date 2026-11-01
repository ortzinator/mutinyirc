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

        public IConnection Current { get; private set; }

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

        /// <summary>Tells the connection in use that it refused the server's certificate.</summary>
        public void RefuseCurrentCertificate() =>
            Current.ConnectFailed += Raise.With(this,
                new ConnectFailedEventArgs(ConnectError.CertificateRejected));

        /// <summary>Tells the connection in use that a live session dropped.</summary>
        public void DropCurrentConnection() =>
            Current.ConnectionLost += Raise.With(this,
                new DisconnectEventArgs(DisconnectReason.RemoteHostClosedConnection));

        /// <summary>Feeds the welcome reply, which is what ends a run of tries.</summary>
        public void Register() =>
            Current.Listener.Parse(":irc.example 001 nick :Welcome to the network");
    }

    private static ConnectionArgs Candidate(string host, int port) =>
        new("nick", host, false) { Port = port };

    private WalkingServer _server;
    private List<int> _reconnectAttempts;

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

    // ── Refused certificates ─────────────────────────────────────────────────

    [Test]
    public void ARefusedCertificate_MovesToTheNextEntryPointAtOnce()
    {
        Build(Candidate("refused.example", 6697), Candidate("healthy.example", 6697));

        _server.RefuseCurrentCertificate();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "healthy.example:6697" }));
            Assert.That(_reconnectAttempts, Is.Empty);
        });
    }

    [Test]
    public void ARefusedEntryPoint_IsSkippedAfterALaterDrop()
    {
        Build(Candidate("refused.example", 6697), Candidate("healthy.example", 6697));

        _server.RefuseCurrentCertificate();
        _server.Register();
        _server.DropCurrentConnection();
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Url, Is.EqualTo("healthy.example"),
                "the backoff round goes back to the healthy host, not to the refused one");
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1, 2 }));
        });
    }

    [Test]
    public void EveryEntryPointRefused_StopsWithoutABackoffRound()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        int stops = 0;
        _server.EveryEntryPointRefused += (_, _) => stops++;

        _server.RefuseCurrentCertificate();
        _server.RefuseCurrentCertificate();

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.EqualTo(1));
            Assert.That(_reconnectAttempts, Is.Empty, "waiting would only get the same refusals");
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697" }));
        });
    }

    [Test]
    public void OneEntryPoint_ARefusal_StopsWithoutABackoffRound()
    {
        Build(Candidate("a.example", 6697));
        int stops = 0;
        _server.EveryEntryPointRefused += (_, _) => stops++;

        _server.RefuseCurrentCertificate();

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.EqualTo(1));
            Assert.That(_reconnectAttempts, Is.Empty);
        });
    }

    [Test]
    public void ARefusedAndADownHost_TheDownHostGetsABackoffRoundWithoutTheRefusedOne()
    {
        Build(Candidate("refused.example", 6697), Candidate("down.example", 6697));
        int stops = 0;
        _server.EveryEntryPointRefused += (_, _) => stops++;

        _server.RefuseCurrentCertificate();
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
            Assert.That(_server.Url, Is.EqualTo("down.example"),
                "the backoff round dials the down host, and skips the refused one");
            Assert.That(stops, Is.Zero);
        });
    }

    [Test]
    public void Reconnect_AfterAStop_DialsTheFirstEntryPointAgain()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.RefuseCurrentCertificate();
        _server.RefuseCurrentCertificate();

        _server.Reconnect();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Url, Is.EqualTo("a.example"));
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697", "a.example:6697" }));
        });
    }

    [Test]
    public void Reconnect_ClearsTheSkipList()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.RefuseCurrentCertificate();
        _server.RefuseCurrentCertificate();

        _server.Reconnect();
        _server.FailCurrentDial();

        Assert.That(_server.Url, Is.EqualTo("b.example"),
            "a new attempt walks every entry point again");
    }

    [Test]
    public void DialEntryPoint_DialsThatEntryPointAndClearsTheSkipList()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.RefuseCurrentCertificate();
        _server.RefuseCurrentCertificate();

        _server.DialEntryPoint("b.example", 6697);
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697", "b.example:6697" }),
                "b is dialled at once");
            Assert.That(_server.Url, Is.EqualTo("a.example"),
                "a failure on b wraps to a, which is no longer skipped");
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
        });
    }

    [Test]
    public void ChangeServer_ClearsTheSkipList()
    {
        Build(Candidate("a.example", 6697));
        _server.RefuseCurrentCertificate();

        _server.ChangeServer(Candidate("chosen.example", 6697));
        _server.FailCurrentDial();

        Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }),
            "the new host is not skipped because the old one refused its certificate");
    }

    [Test]
    public void DialEntryPoint_AnEntryPointNotInTheList_Throws()
    {
        Build(Candidate("a.example", 6697));

        Assert.That(() => _server.DialEntryPoint("other.example", 6697), Throws.ArgumentException);
    }

    // ── A save replaces the entry points ─────────────────────────────────────

    private static ConnectionArgs TlsCandidate(string host, int port) =>
        new("nick", host, true) { Port = port };

    [Test]
    public void ReplaceEntryPoints_KeepsTheSessionUp()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.Register();
        IConnection live = _server.Current;

        _server.ReplaceEntryPoints(new[] { Candidate("b.example", 6697) });

        Assert.Multiple(() =>
        {
            Assert.That(_server.Current, Is.SameAs(live), "a save never rebuilds the connection");
            A.CallTo(() => live.Disconnect(A<string>._)).MustNotHaveHappened();
            Assert.That(_server.Dialled, Is.Empty, "a save never dials");
        });
    }

    [Test]
    public void ReplaceEntryPoints_AFailureGoesToTheEntryPointAfterTheCurrentHostInTheNewOrder()
    {
        Build(Candidate("a.example", 6697),
              Candidate("b.example", 6697),
              Candidate("c.example", 6697));
        _server.FailCurrentDial();

        _server.ReplaceEntryPoints(new[]
        {
            Candidate("c.example", 6697),
            Candidate("b.example", 6697),
            Candidate("a.example", 6697)
        });
        _server.FailCurrentDial();

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697", "a.example:6697" }));
    }

    [Test]
    public void ReplaceEntryPoints_AnAddedEntryPointIsDialledOnFailure()
    {
        Build(Candidate("a.example", 6697));

        _server.ReplaceEntryPoints(new[] { Candidate("a.example", 6697), Candidate("new.example", 6667) });
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "new.example:6667" }));
            Assert.That(_reconnectAttempts, Is.Empty);
        });
    }

    [Test]
    public void ReplaceEntryPoints_CurrentHostRemoved_AFailureStartsAgainFromTheTop()
    {
        Build(Candidate("a.example", 6697),
              Candidate("b.example", 6697),
              Candidate("c.example", 6697));
        _server.FailCurrentDial();

        _server.ReplaceEntryPoints(new[] { Candidate("c.example", 6697), Candidate("a.example", 6697) });
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697", "c.example:6697" }));
            Assert.That(_reconnectAttempts, Is.Empty);
        });
    }

    [Test]
    public void ReplaceEntryPoints_CurrentHostRemoved_ADropGoesToTheTop()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.Register();

        _server.ReplaceEntryPoints(new[] { Candidate("c.example", 6667), Candidate("b.example", 6697) });
        _server.DropCurrentConnection();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Url, Is.EqualTo("c.example"));
            Assert.That(_server.Port, Is.EqualTo(6667));
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
        });
    }

    [Test]
    public void ReplaceEntryPoints_CurrentEntryPointEdited_ADropUsesTheEdit()
    {
        Build(Candidate("a.example", 6667));
        _server.Register();

        _server.ReplaceEntryPoints(new[] { TlsCandidate("a.example", 6697) });
        _server.DropCurrentConnection();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Port, Is.EqualTo(6697));
            Assert.That(_server.Current.ConnectionData.Ssl, Is.True);
        });
    }

    [Test]
    public void ReplaceEntryPoints_CurrentEntryPointUnchanged_ADropKeepsTheConnection()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.Register();
        IConnection live = _server.Current;

        _server.ReplaceEntryPoints(new[] { Candidate("b.example", 6697), Candidate("a.example", 6697) });
        _server.DropCurrentConnection();

        Assert.That(_server.Current, Is.SameAs(live));
    }

    [Test]
    public void ReplaceEntryPoints_TheSkipListKeepsAnEntryPointThatIsStillListed()
    {
        Build(Candidate("refused.example", 6697), Candidate("healthy.example", 6697));
        _server.RefuseCurrentCertificate();

        _server.ReplaceEntryPoints(new[]
        {
            Candidate("healthy.example", 6697),
            Candidate("refused.example", 6697)
        });
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Url, Is.EqualTo("healthy.example"),
                "the refused entry point is still skipped, so the round wraps to the healthy host");
            Assert.That(_reconnectAttempts, Is.EqualTo(new[] { 1 }));
        });
    }

    [Test]
    public void ReplaceEntryPoints_TheSkipListDropsAnEntryPointThatIsNoLongerListed()
    {
        Build(Candidate("refused.example", 6697), Candidate("healthy.example", 6697));
        _server.RefuseCurrentCertificate();

        // The port edit makes the refused host a different entry point.
        _server.ReplaceEntryPoints(new[]
        {
            Candidate("healthy.example", 6697),
            Candidate("refused.example", 7000)
        });
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled,
                        Is.EqualTo(new[] { "healthy.example:6697", "refused.example:7000" }));
            Assert.That(_reconnectAttempts, Is.Empty);
        });
    }

    [Test]
    public void ReplaceEntryPoints_Reconnect_DialsTheTopOfTheNewList()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        _server.ReplaceEntryPoints(new[] { Candidate("b.example", 6697), Candidate("a.example", 6697) });
        _server.Reconnect();

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697" }));
    }

    [Test]
    public async Task ReplaceEntryPoints_WhileAReconnectWaits_TheReconnectDialsTheNewList()
    {
        // The save comes after the drop, so the reconnect waits already. It must not dial the
        // old host: the current host is not in the new list, so the walk starts at the top.
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        _server.Register();
        _server.DropCurrentConnection();

        _server.ReplaceEntryPoints(new[]
        {
            TlsCandidate("c.example", 7000),
            Candidate("b.example", 6697)
        });
        await Task.Delay(TimeSpan.FromSeconds(5));

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Is.EqualTo(new[] { "c.example:7000" }));
            Assert.That(_server.Current.ConnectionData.Ssl, Is.True);
        });
    }

    [Test]
    public void ReplaceEntryPoints_ReorderOrAnotherEntryPointEdited_SaysTheCurrentOneIsUnchanged()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        bool changed = _server.ReplaceEntryPoints(new[]
        {
            Candidate("b.example", 7000),
            Candidate("A.EXAMPLE", 6697)
        });

        Assert.That(changed, Is.False);
    }

    [TestCase("a.example", 7000, false, TestName = "ReplaceEntryPoints_PortEdited_SaysTheCurrentOneChanged")]
    [TestCase("a.example", 6697, true, TestName = "ReplaceEntryPoints_TlsChanged_SaysTheCurrentOneChanged")]
    [TestCase("other.example", 6697, false, TestName = "ReplaceEntryPoints_HostEdited_SaysTheCurrentOneChanged")]
    public void ReplaceEntryPoints_CurrentEntryPointEdited_SaysItChanged(string host, int port, bool tls)
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        bool changed = _server.ReplaceEntryPoints(new[]
        {
            new ConnectionArgs("nick", host, tls) { Port = port },
            Candidate("b.example", 6697)
        });

        Assert.That(changed, Is.True);
    }

    [Test]
    public void ReplaceEntryPoints_CurrentEntryPointRemoved_SaysItChanged()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));

        bool changed = _server.ReplaceEntryPoints(new[] { Candidate("b.example", 6697) });

        Assert.That(changed, Is.True);
    }

    // ── No entry points left ─────────────────────────────────────────────────

    [Test]
    public void NoEntryPoints_ADrop_StopsWithoutAReconnect()
    {
        Build(Candidate("a.example", 6697));
        _server.Register();
        int stops = 0;
        _server.NoEntryPoints += (_, _) => stops++;

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());
        _server.DropCurrentConnection();

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.EqualTo(1));
            Assert.That(_reconnectAttempts, Is.Empty);
            Assert.That(_server.Dialled, Is.Empty);
        });
    }

    [Test]
    public void NoEntryPoints_AFailedDial_StopsWithoutAReconnect()
    {
        Build(Candidate("a.example", 6697), Candidate("b.example", 6697));
        int stops = 0;
        _server.NoEntryPoints += (_, _) => stops++;

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());
        _server.FailCurrentDial();

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.EqualTo(1));
            Assert.That(_reconnectAttempts, Is.Empty);
            Assert.That(_server.Dialled, Is.Empty);
        });
    }

    [Test]
    public void NoEntryPoints_WhileAReconnectWaits_StopsAtOnce()
    {
        Build(Candidate("a.example", 6697));
        _server.Register();
        _server.DropCurrentConnection();
        int stops = 0;
        _server.NoEntryPoints += (_, _) => stops++;

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());

        Assert.That(stops, Is.EqualTo(1));
    }

    [Test]
    public void NoEntryPoints_WhileTheSessionIsUp_DoesNotStop()
    {
        Build(Candidate("a.example", 6697));
        _server.Register();
        IConnection live = _server.Current;
        int stops = 0;
        _server.NoEntryPoints += (_, _) => stops++;

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.Zero);
            A.CallTo(() => live.Disconnect(A<string>._)).MustNotHaveHappened();
        });
    }

    [Test]
    public void NoEntryPoints_Reconnect_StopsWithoutADial()
    {
        Build(Candidate("a.example", 6697));
        int stops = 0;
        _server.NoEntryPoints += (_, _) => stops++;

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());
        _server.Reconnect();

        Assert.Multiple(() =>
        {
            Assert.That(stops, Is.EqualTo(1));
            Assert.That(_server.Dialled, Is.Empty);
        });
    }

    [Test]
    public void NoEntryPoints_ThenEntryPointsAgain_ReconnectDialsThem()
    {
        Build(Candidate("a.example", 6697));

        _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());
        _server.ReplaceEntryPoints(new[] { Candidate("b.example", 6697) });
        _server.Reconnect();

        Assert.That(_server.Dialled, Is.EqualTo(new[] { "b.example:6697" }));
    }

    [Test]
    public void NoEntryPoints_SaysTheCurrentOneChanged()
    {
        Build(Candidate("a.example", 6697));

        bool changed = _server.ReplaceEntryPoints(Array.Empty<ConnectionArgs>());

        Assert.That(changed, Is.True);
    }

    // ── Guards ───────────────────────────────────────────────────────────────

    [Test]
    public void Constructor_NoEntryPoints_Throws()
    {
        Assert.That(() => new Server(Array.Empty<ConnectionArgs>()),
            Throws.ArgumentException);
    }
}
