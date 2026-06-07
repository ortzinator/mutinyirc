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

        // --- Regression: events for untracked channels must not throw KeyNotFoundException (gap #2) ---

        [Test]
        public void PublicMessage_ForUntrackedChannel_DoesNotThrowAndIsIgnored()
        {
            bool fired = false;
            _server.ChannelMessaged += (_, _) => fired = true;

            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h PRIVMSG #notjoined :hello"));
            Assert.IsFalse(fired, "A message for a channel we don't track must be ignored");
        }

        [Test]
        public void Kick_ForUntrackedChannel_DoesNotThrowAndIsIgnored()
        {
            bool fired = false;
            _server.Kick += (_, _) => fired = true;

            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h KICK #notjoined victim :gone"));
            Assert.IsFalse(fired, "A kick for a channel we don't track must be ignored");
        }

        [Test]
        public void ChannelModeChange_ForUntrackedChannel_DoesNotThrowAndIsIgnored()
        {
            bool fired = false;
            _server.ChannelModeChange += (_, _) => fired = true;

            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h MODE #notjoined +o someone"));
            Assert.IsFalse(fired, "A mode change for a channel we don't track must be ignored");
        }

        [Test]
        public void Part_ForUntrackedChannel_DoesNotThrowAndIsIgnored()
        {
            bool fired = false;
            _server.Part += (_, _) => fired = true;

            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h PART #notjoined :bye"));
            Assert.IsFalse(fired, "A part for a channel we don't track must be ignored");
        }

        [Test]
        public void PublicNotice_ForTrackedChannel_FiresChannelOnNotice()
        {
            var chan = _server.CreateChannel("#test");
            chan.Membership = ChannelMembership.Joined;

            UserMessageEventArgs captured = null;
            chan.OnNotice += (_, e) => captured = e;

            _server.Connection.Listener.Parse(":bob!u@h NOTICE #test :heads up");

            Assert.IsNotNull(captured, "A channel notice must route to the channel's OnNotice");
            Assert.AreEqual("bob", captured.User.Nick);
            Assert.AreEqual("heads up", captured.Message);
        }

        [Test]
        public void PublicNotice_ForUntrackedChannel_DoesNotThrowAndCreatesNoChannel()
        {
            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h NOTICE #notjoined :heads up"));
            Assert.IsFalse(_server.Channels.ContainsKey("#notjoined"),
                "A notice must not create a channel we never joined");
        }

        [Test]
        public void PrivateNotice_FromUserInCommonChannel_RoutesToChannelNotServerWindow()
        {
            var chan = _server.CreateChannel("#test");
            chan.Membership = ChannelMembership.Joined;
            chan.AddNick(User.FromNames("bob"));

            UserMessageEventArgs channelNotice = null;
            chan.OnNotice += (_, e) => channelNotice = e;
            bool serverWindowFired = false;
            _server.PrivateNotice += (_, _) => serverWindowFired = true;

            _server.Connection.Listener.Parse(":bob!u@h NOTICE test :psst");

            Assert.IsNotNull(channelNotice, "A notice from a shared-channel user must route to that channel");
            Assert.AreEqual("bob", channelNotice.User.Nick);
            Assert.AreEqual("psst", channelNotice.Message);
            Assert.IsFalse(serverWindowFired, "A routed notice must not also hit the server window");
        }

        [Test]
        public void PrivateNotice_FromUserInMultipleCommonChannels_RoutesToEach()
        {
            int count = 0;
            foreach (string name in new[] { "#one", "#two" })
            {
                var chan = _server.CreateChannel(name);
                chan.Membership = ChannelMembership.Joined;
                chan.AddNick(User.FromNames("bob"));
                chan.OnNotice += (_, _) => count++;
            }

            _server.Connection.Listener.Parse(":bob!u@h NOTICE test :psst");

            Assert.AreEqual(2, count, "A notice must appear in every channel shared with the sender");
        }

        [Test]
        public void PrivateNotice_FromUserWithNoCommonChannel_FallsBackToServerWindow()
        {
            var chan = _server.CreateChannel("#test");
            chan.Membership = ChannelMembership.Joined;

            bool serverWindowFired = false;
            _server.PrivateNotice += (_, _) => serverWindowFired = true;

            _server.Connection.Listener.Parse(":stranger!u@h NOTICE test :psst");

            Assert.IsTrue(serverWindowFired,
                "A notice from a user we share no channel with must fall back to the server window");
        }

        // --- Only the join lifecycle creates channels; content events for an untracked
        //     (but otherwise valid) channel must not materialize a phantom channel ---

        [Test]
        public void Action_ForUntrackedChannel_DoesNotCreateChannelAndIsIgnored()
        {
            bool fired = false;
            _server.UserAction += (_, _) => fired = true;

            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h PRIVMSG #notjoined :ACTION waves"));
            Assert.IsFalse(fired, "An action for a channel we don't track must be ignored");
            Assert.IsFalse(_server.Channels.ContainsKey("#notjoined"),
                "An action must not create a channel we never joined");
        }

        [Test]
        public void Topic_ForUntrackedChannel_DoesNotCreateChannel()
        {
            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":server.name 332 test #notjoined :a topic"));
            Assert.IsFalse(_server.Channels.ContainsKey("#notjoined"),
                "A topic must not create a channel we never joined");
        }

        // --- Regression: invalid channel names make CreateChannel return null; callers must not NRE (gap #5) ---

        [Test]
        public void Topic_ForInvalidChannelName_DoesNotThrowAndCreatesNoChannel()
        {
            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":server.name 332 test badname :a topic"));
            Assert.IsFalse(_server.Channels.ContainsKey("badname"),
                "An invalid channel name must not be added to the channel list");
        }

        [Test]
        public void Join_ForInvalidChannelName_DoesNotThrowAndCreatesNoChannel()
        {
            Assert.DoesNotThrow(() =>
                _server.Connection.Listener.Parse(":bob!u@h JOIN badname"));
            Assert.IsFalse(_server.Channels.ContainsKey("badname"),
                "An invalid channel name must not be added to the channel list");
        }

        [Test]
        public void JoinChannel_WithInvalidChannelName_ReturnsNullAndCreatesNoChannel()
        {
            Channel result = null;
            Assert.DoesNotThrow(() => result = _server.JoinChannel("badname"));
            Assert.IsNull(result, "Joining an invalid channel name must return null, not throw");
            Assert.IsFalse(_server.Channels.ContainsKey("badname"),
                "An invalid channel name must not be added to the channel list");
        }
    }
}
