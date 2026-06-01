using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Verifies the private-message routing rules on <see cref="Server"/>:
    ///   session creation, the service-nick bypass, and the action variant.
    /// </summary>
    [TestFixture]
    public class PrivateMessageRoutingTests
    {
        private Server _server;

        [SetUp]
        public void Setup()
        {
            var args = new ConnectionArgs("test", "irc.fake.com", false);
            var connMock = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
            _server = new Server(connMock);
        }

        [TearDown]
        public void Teardown()
        {
            _server = null;
        }

        [Test]
        public void GetOrCreatePM_NewUser_CreatesSessionAndFiresAdded()
        {
            PrivateMessageSession added = null;
            _server.PrivateMessageSessionAdded += (_, e) => added = e.PrivateMessageSession;

            var session = _server.GetOrCreatePM(new User { Nick = "alice" });

            Assert.IsNotNull(session);
            Assert.AreSame(session, added);
            Assert.AreEqual("alice", session.User.Nick);
            Assert.Contains(session, _server.PMSessions);
        }

        [Test]
        public void GetOrCreatePM_ExistingUser_ReturnsSameSession()
        {
            var first = _server.GetOrCreatePM(new User { Nick = "alice" });
            var second = _server.GetOrCreatePM(new User { Nick = "alice" });

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _server.PMSessions.Count);
        }

        [Test]
        public void GetOrCreatePM_ServiceNick_ReturnsNullAndDoesNotCreate()
        {
            _server.ServiceNicks.Add("NickServ");

            PrivateMessageSession session = _server.GetOrCreatePM("NickServ");

            Assert.IsNull(session);
            Assert.AreEqual(0, _server.PMSessions.Count);
        }

        [Test]
        public void IncomingPM_FromServiceNick_FiresServiceMessageReceived_NoSession()
        {
            _server.ServiceNicks.Add("NickServ");

            UserMessageEventArgs serviceArgs = null;
            _server.ServiceMessageReceived += (_, e) => serviceArgs = e;
            bool sessionAdded = false;
            _server.PrivateMessageSessionAdded += (_, _) => sessionAdded = true;

            _server.Connection.Listener.Parse(":NickServ!services@host PRIVMSG test :You are now identified");

            Assert.IsNotNull(serviceArgs, "ServiceMessageReceived must fire for a service-nick PRIVMSG");
            Assert.AreEqual("NickServ", serviceArgs.User.Nick);
            Assert.IsTrue(serviceArgs.Message.Contains("identified"));
            Assert.IsFalse(sessionAdded, "Service messages must not create a PM session");
            Assert.AreEqual(0, _server.PMSessions.Count);
        }

        [Test]
        public void IncomingPM_FromRegularUser_RoutesToSession()
        {
            string received = null;
            _server.PrivateMessageSessionAdded += (_, e) =>
                e.PrivateMessageSession.MessageReceived += (_, m) => received = m.Data;

            _server.Connection.Listener.Parse(":alice!a@host PRIVMSG test :hey there");

            Assert.AreEqual("hey there", received);
            Assert.AreEqual(1, _server.PMSessions.Count);
        }

        [Test]
        public void IncomingPrivateAction_FromRegularUser_RoutesToSessionActionReceived()
        {
            string received = null;
            _server.PrivateMessageSessionAdded += (_, e) =>
                e.PrivateMessageSession.ActionReceived += (_, m) => received = m.Data;

            _server.Connection.Listener.Parse(":alice!a@host PRIVMSG test :ACTION waves");

            Assert.AreEqual("waves", received);
        }

        [Test]
        public void IncomingPrivateAction_FromServiceNick_FiresServiceActionReceived_NoSession()
        {
            _server.ServiceNicks.Add("ChanServ");

            UserMessageEventArgs serviceArgs = null;
            _server.ServiceActionReceived += (_, e) => serviceArgs = e;
            bool sessionAdded = false;
            _server.PrivateMessageSessionAdded += (_, _) => sessionAdded = true;

            _server.Connection.Listener.Parse(":ChanServ!s@host PRIVMSG test :ACTION pokes you");

            Assert.IsNotNull(serviceArgs);
            Assert.AreEqual("ChanServ", serviceArgs.User.Nick);
            Assert.IsFalse(sessionAdded);
        }

        [Test]
        public void RemovePM_RemovesSession()
        {
            var session = _server.GetOrCreatePM(new User { Nick = "alice" });

            _server.RemovePM(session);

            Assert.AreEqual(0, _server.PMSessions.Count);
        }

        [Test]
        public void RemovePM_UnknownSession_LeavesTrackedSessionsIntact()
        {
            var tracked = _server.GetOrCreatePM(new User { Nick = "alice" });
            var stray = new PrivateMessageSession(_server, new User { Nick = "ghost" });

            _server.RemovePM(stray);

            Assert.AreEqual(1, _server.PMSessions.Count);
            Assert.Contains(tracked, _server.PMSessions);
        }

        [Test]
        public void MessageService_FiresServiceMessageSent()
        {
            UserMessageEventArgs sent = null;
            _server.ServiceMessageSent += (_, e) => sent = e;

            _server.MessageService("NickServ", "identify hunter2");

            Assert.IsNotNull(sent, "MessageService must fire ServiceMessageSent so the host can echo it");
            Assert.AreEqual("NickServ", sent.User.Nick);
            Assert.AreEqual("identify hunter2", sent.Message);
        }

        [Test]
        public void ServiceNickMatching_IsCaseInsensitive()
        {
            _server.ServiceNicks.Add("ChanServ");

            Assert.IsNull(_server.GetOrCreatePM("chanserv"));
            Assert.IsNull(_server.GetOrCreatePM("CHANSERV"));
            Assert.IsNotNull(_server.GetOrCreatePM("alice"));
        }
    }
}