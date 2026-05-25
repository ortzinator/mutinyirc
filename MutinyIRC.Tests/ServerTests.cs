using System;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class ServerTests
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
        public void SetupConnection_GivenConnectionArgsAndNickNotSet_ArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(delegate () {
                _server.SetupConnection(new ConnectionArgs());
            });
        }

        [Test]
        public void Connection_SetToNull_ArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(delegate () {
                _server.Connection = null;
            });
        }

        [Test]
        public void InChannel_JoinedChannel_ReturnsTrue()
        {
            var chan = new Channel(_server, "#test") { Membership = ChannelMembership.Joined };
            _server.Channels.Add("#test", chan);

            Assert.IsTrue(_server.InChannel("#test"));
        }

        [Test]
        public void InChannel_UnknownChannel_ReturnsFalse()
        {
            Assert.IsFalse(_server.InChannel("#nope"));
        }

        [Test]
        public void InChannel_NotJoinedChannel_ReturnsFalse()
        {
            var chan = new Channel(_server, "#test") { Membership = ChannelMembership.Joining };
            _server.Channels.Add("#test", chan);

            Assert.IsFalse(_server.InChannel("#test"),
                "A channel awaiting its JOIN echo must not report as joined");
        }

        [Test]
        public void InChannel_NotJoinedChannel_DoesNotRemoveChannelOrFireChannelRemoved()
        {
            var chan = new Channel(_server, "#test") { Membership = ChannelMembership.Joining };
            _server.Channels.Add("#test", chan);

            bool removedFired = false;
            EventHandler<ChannelEventArgs> handler = (_, _) => removedFired = true;
            Server.ChannelRemoved += handler;
            try
            {
                _server.InChannel("#test");
            }
            finally
            {
                Server.ChannelRemoved -= handler;
            }

            Assert.IsTrue(_server.Channels.ContainsKey("#test"),
                "InChannel must be a pure query and not prune the channel");
            Assert.IsFalse(removedFired, "InChannel must not fire ChannelRemoved");
        }

        [Test]
        public void BadChannelKeyError_ForJoiningChannel_RemovesChannelAndFiresChannelRemoved()
        {
            var chan = _server.CreateChannel("#secret");
            chan.Membership = ChannelMembership.Joining;

            Channel removed = null;
            EventHandler<ChannelEventArgs> handler = (_, e) => removed = e.Channel;
            Server.ChannelRemoved += handler;
            try
            {
                _server.Connection.Listener.Parse(":server.name 475 test #secret :Cannot join channel (+k)");
            }
            finally
            {
                Server.ChannelRemoved -= handler;
            }

            Assert.IsFalse(_server.Channels.ContainsKey("#secret"),
                "A rejected JOIN must not leave the channel in the manager");
            Assert.IsNotNull(removed, "ChannelRemoved must fire for the pruned channel");
            Assert.AreEqual("#secret", removed.Name);
            Assert.AreEqual(ChannelMembership.NotJoined, chan.Membership);
        }

        [Test]
        public void BadChannelKeyError_ForJoinedChannel_DoesNotRemoveChannel()
        {
            var chan = _server.CreateChannel("#secret");
            chan.Membership = ChannelMembership.Joined;

            bool removedFired = false;
            EventHandler<ChannelEventArgs> handler = (_, _) => removedFired = true;
            Server.ChannelRemoved += handler;
            try
            {
                _server.Connection.Listener.Parse(":server.name 475 test #secret :Cannot join channel (+k)");
            }
            finally
            {
                Server.ChannelRemoved -= handler;
            }

            Assert.IsTrue(_server.Channels.ContainsKey("#secret"),
                "A stray rejection must not evict an already-joined channel");
            Assert.IsFalse(removedFired);
            Assert.AreEqual(ChannelMembership.Joined, chan.Membership);
        }

        [Test]
        public void BadChannelKeyError_AlsoPropagatesViaErrorMessageReceived()
        {
            var chan = _server.CreateChannel("#secret");
            chan.Membership = ChannelMembership.Joining;

            ErrorMessageEventArgs received = null;
            _server.ErrorMessageRecieved += (_, e) => received = e;

            _server.Connection.Listener.Parse(":server.name 475 test #secret :Cannot join channel (+k)");

            Assert.IsNotNull(received, "The error must still surface to ErrorMessageRecieved subscribers");
            Assert.AreEqual(ReplyCode.ERR_BADCHANNELKEY, received.Code);
        }
    }
}
