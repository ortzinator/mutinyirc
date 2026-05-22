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
    }
}
