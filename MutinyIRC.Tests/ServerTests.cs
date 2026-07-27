using System;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests;

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
        Assert.Throws<ArgumentNullException>(delegate ()
        {
            _server.SetupConnection(new ConnectionArgs());
        });
    }

    [Test]
    public void Connection_SetToNull_ArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(delegate ()
        {
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
        _server.ChannelRemoved += (_, _) => removedFired = true;

        _server.InChannel("#test");

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
        _server.ChannelRemoved += (_, e) => removed = e.Channel;

        _server.Connection.Listener.Parse(":server.name 475 test #secret :Cannot join channel (+k)");

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
        _server.ChannelRemoved += (_, _) => removedFired = true;

        _server.Connection.Listener.Parse(":server.name 475 test #secret :Cannot join channel (+k)");

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
    public void PrivateNotice_FromUser_FiresPrivateNoticeEvent()
    {
        // The server surfaces every private notice via PrivateNotice; the UI layer decides
        // which window (active vs. server) it lands in, so the channel is never involved here.
        var chan = _server.CreateChannel("#test");
        chan.Membership = ChannelMembership.Joined;
        chan.AddNick(User.FromNames("bob"));

        UserMessageEventArgs captured = null;
        chan.OnNotice += (_, _) => Assert.Fail("A private notice must not route to a channel");
        _server.PrivateNotice += (_, e) => captured = e;

        _server.Connection.Listener.Parse(":bob!u@h NOTICE test :psst");

        Assert.IsNotNull(captured, "A private notice must fire PrivateNotice");
        Assert.AreEqual("bob", captured.User.Nick);
        Assert.AreEqual("psst", captured.Message);
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

    // --- Away status: server-confirmed via 301/305/306 ---

    [Test]
    public void AwayReply_FromServer_FiresAwayReplyReceivedWithNickAndMessage()
    {
        AwayEventArgs captured = null;
        _server.AwayReplyReceived += (_, e) => captured = e;

        _server.Connection.Listener.Parse(":server.name 301 test Buster :Gone fishing");

        Assert.IsNotNull(captured, "An RPL_AWAY must surface via AwayReplyReceived");
        Assert.AreEqual("Buster", captured.Nick);
        Assert.AreEqual("Gone fishing", captured.AwayMessage);
    }

    [Test]
    public void NowAway_FromServer_SetsIsAwayAndFiresWentAway()
    {
        bool fired = false;
        _server.WentAway += (_, _) => fired = true;
        Assert.IsFalse(_server.IsAway, "A fresh server starts not away");

        _server.Connection.Listener.Parse(":server.name 306 test :You have been marked as being away");

        Assert.IsTrue(_server.IsAway, "RPL_NOWAWAY must mark the server away");
        Assert.IsTrue(fired, "RPL_NOWAWAY must fire WentAway");
    }

    [Test]
    public void UnAway_FromServer_ClearsIsAwayAndFiresCameBack()
    {
        _server.Connection.Listener.Parse(":server.name 306 test :You have been marked as being away");
        Assert.IsTrue(_server.IsAway, "Precondition: server is away");

        bool fired = false;
        _server.CameBack += (_, _) => fired = true;

        _server.Connection.Listener.Parse(":server.name 305 test :You are no longer marked as being away");

        Assert.IsFalse(_server.IsAway, "RPL_UNAWAY must clear the away status");
        Assert.IsTrue(fired, "RPL_UNAWAY must fire CameBack");
    }
}
