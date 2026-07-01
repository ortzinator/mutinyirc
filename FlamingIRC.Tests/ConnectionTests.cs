using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using FakeItEasy;

namespace FlamingIRC.Tests
{
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
            // backoff). A negative PingTimeout forces the timeout branch if the guard were missing,
            // so a raised ConnectionLost here would mean the guard let a dead-link tick tear down.
            _connection.PingTimeout = TimeSpan.FromSeconds(-1);
            var lost = false;
            _connection.ConnectionLost += (_, _) => lost = true;

            Assert.IsFalse(_connection.Connected, "precondition: a fresh Connection is not connected");
            _connection.SendKeepAlive();

            Assert.IsFalse(lost, "a keep-alive tick on a disconnected link must not raise ConnectionLost");
        }
    }
}