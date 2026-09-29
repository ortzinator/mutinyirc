using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using FakeItEasy;

namespace FlamingIRC.Tests;

[TestFixture]
public class ConnectionTests
{
    private Connection _connection;
    private ConnectionArgs _connectionArgs;

    [OneTimeSetUp]
    public void SetupMethods()
    {
    }

    [OneTimeTearDown]
    public void TearDownMethods()
    {
    }

    [SetUp]
    public void SetupTest()
    {
        _connectionArgs = new ConnectionArgs();
        _connection = new Connection(_connectionArgs, false, false);
    }

    [TearDown]
    public void TearDownTest()
    {
        _connection = null;
    }

    [Test]
    public void OnReceiveLine_()
    {
    }

    private static readonly TimeSpan KeepAlive = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(90);

    [Test]
    public void EvaluateKeepAlive_RecentTraffic_DoesNothing()
    {
        Assert.AreEqual(Connection.KeepAliveAction.None,
            Connection.EvaluateKeepAlive(TimeSpan.FromSeconds(10), KeepAlive, PingTimeout));
    }

    [Test]
    public void EvaluateKeepAlive_PastKeepAliveButWithinTimeout_SendsPing()
    {
        Assert.AreEqual(Connection.KeepAliveAction.Ping,
            Connection.EvaluateKeepAlive(TimeSpan.FromSeconds(45), KeepAlive, PingTimeout));
    }

    [Test]
    public void EvaluateKeepAlive_SilencePastTimeout_DeclaresTimeout()
    {
        Assert.AreEqual(Connection.KeepAliveAction.Timeout,
            Connection.EvaluateKeepAlive(TimeSpan.FromSeconds(120), KeepAlive, PingTimeout));
    }

    [Test]
    public void EvaluateKeepAlive_ExactlyAtKeepAlive_DoesNotPingYet()
    {
        // The threshold is exclusive (> not >=), so the boundary stays quiet for one more tick.
        Assert.AreEqual(Connection.KeepAliveAction.None,
            Connection.EvaluateKeepAlive(KeepAlive, KeepAlive, PingTimeout));
    }

    [Test]
    public void EvaluateKeepAlive_ExactlyAtTimeout_StillPingsNotTimesOut()
    {
        Assert.AreEqual(Connection.KeepAliveAction.Ping,
            Connection.EvaluateKeepAlive(PingTimeout, KeepAlive, PingTimeout));
    }

    [Test]
    public void SendKeepAlive_WhenNotConnected_DoesNotTearDown()
    {
        // The timer keeps ticking while the link is dead (before Connect(), during reconnect
        // backoff). Backdate traffic well past the timeout so the timeout branch would fire if
        // the guard were missing; a raised ConnectionLost would then mean a dead-link tick tore
        // the connection down.
        _connection.SetLastTrafficForTest(DateTime.Now - TimeSpan.FromSeconds(120));
        var lost = false;
        _connection.ConnectionLost += (_, _) => lost = true;

        Assert.IsFalse(_connection.Connected, "precondition: a fresh Connection is not connected");
        _connection.SendKeepAlive();

        Assert.IsFalse(lost, "a keep-alive tick on a disconnected link must not raise ConnectionLost");
    }

    private const string Welcome = ":irc.test 001 me :Welcome to the Internet Relay Network";

    [Test]
    public void Registered_AfterWelcome_IsTrue()
    {
        _connection.Listener.Parse(Welcome);

        Assert.IsTrue(_connection.Registered, "RPL_WELCOME completes registration");
    }

    [Test]
    public void Registered_AfterDisconnect_IsFalse()
    {
        _connection.Listener.Parse(Welcome);

        _connection.Disconnect(DisconnectReason.RemoteHostClosedConnection);

        Assert.IsFalse(_connection.Registered, "registration belongs to one session and must end with it");
    }

    [Test]
    public void Registered_AfterReconnectAndSecondWelcome_IsTrueAgain()
    {
        // Server reuses one Connection across reconnects to the same host, so the second
        // session's 001 must set the flag again.
        _connection.Listener.Parse(Welcome);
        _connection.Disconnect(DisconnectReason.RemoteHostClosedConnection);

        _connection.Listener.Parse(Welcome);

        Assert.IsTrue(_connection.Registered, "a reused Connection must register again on the next session's 001");
    }

    [Test]
    public void Registered_IsFalseWhenConnectionLostRaised()
    {
        // Reconnect logic runs inside ConnectionLost, so it must already see the session as ended.
        _connection.Listener.Parse(Welcome);
        bool? registeredDuringLost = null;
        _connection.ConnectionLost += (_, _) => registeredDuringLost = _connection.Registered;

        _connection.Disconnect(DisconnectReason.RemoteHostClosedConnection);

        Assert.AreEqual(false, registeredDuringLost, "ConnectionLost handlers must see Registered already cleared");
    }

    [Test]
    public void PingTimeout_SetBelowKeepAliveInterval_Throws()
    {
        // A timeout at or under the keep-alive interval fires before any PING is sent, silently
        // defeating half-open detection, so the setter must reject it.
        Assert.Throws<ArgumentOutOfRangeException>(() => _connection.PingTimeout = TimeSpan.FromSeconds(10));
    }

    [Test]
    public void PingTimeout_SetAboveKeepAliveInterval_IsAccepted()
    {
        _connection.PingTimeout = TimeSpan.FromSeconds(45);
        Assert.AreEqual(TimeSpan.FromSeconds(45), _connection.PingTimeout);
    }
}
