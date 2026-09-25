using System.Collections.Generic;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;
using MutinyIRC.Common;
using FakeItEasy;
using FlamingIRC;

namespace MutinyIRC.Tests;

[TestFixture]
public class ChannelTests
{
    private Channel _channel;

    [SetUp]
    public void Setup()
    {
        Server serverMock = A.Fake<Server>();
        _channel = new Channel(serverMock, "#mutiny");
    }

    [TearDown]
    public void Teardown()
    {
        _channel = null;
    }

    [Test]
    public void ShowTopic_TopicReceivedRegistered_EventFires()
    {

        bool eventWasRaised = false;
        const string expected = "Topic here and stuff";

        string topic = string.Empty;
        _channel.TopicReceived += delegate (object sender, Common.DataEventArgs<string> e)
        {
            eventWasRaised = true;
            topic = e.Data;
        };
        _channel.ShowTopic(expected);
        Assert.IsTrue(eventWasRaised, "TopicReceived event was not fired");
        Assert.AreEqual(expected, topic);
    }

    private static Channel CreateChannelWithConnection()
    {
        var fakeSender = A.Fake<ISender>();
        var fakeConn = A.Fake<IConnection>();
        A.CallTo(() => fakeConn.Sender).Returns(fakeSender);
        var server = A.Fake<Server>();
        A.CallTo(() => server.Connection).Returns(fakeConn);
        A.CallTo(() => server.UserNick).Returns("TestUser");
        return new Channel(server, "#mutiny");
    }

    [Test]
    public void Act_Always_FiresOnAction()
    {
        var channel = CreateChannelWithConnection();
        bool onActionFired = false;
        channel.OnAction += (_, _) => onActionFired = true;

        channel.Act("waves");

        Assert.IsTrue(onActionFired, "OnAction must fire when the local user calls Act()");
    }

    [Test]
    public void Act_Always_DoesNotFireMessagedChannel()
    {
        var channel = CreateChannelWithConnection();
        bool messagedChannelFired = false;
        channel.MessagedChannel += (_, _) => messagedChannelFired = true;

        channel.Act("waves");

        Assert.IsFalse(messagedChannelFired,
            "Act() must not fire MessagedChannel; actions are not regular messages");
    }

    [Test]
    [Category("Profile")]
    public void Server_OnNick_Updates_Nick()
    {
        _channel.Users = A.Fake<UserList>();
        User gotUser = new User("Ortzinator", "Ortzinator", "");
        A.CallTo(() => _channel.Users.GetUser(A<User>.Ignored)).Returns(gotUser);

        _channel.Server_OnNick(null, new NickChangeEventArgs(null, "BillNye"));

        Assert.AreEqual(gotUser.Nick, "BillNye");
    }

    [Test]
    public void Server_OnNick_UserInChannel_FiresNickChanged()
    {
        _channel.Users = A.Fake<UserList>();
        User cached = new User("Ortzinator", "Ortzinator", "");
        A.CallTo(() => _channel.Users.GetUser(A<User>.Ignored)).Returns(cached);

        NickChangeEventArgs received = null;
        _channel.NickChanged += (_, e) => received = e;

        var args = new NickChangeEventArgs(new User("Ortzinator", "Ortzinator", ""), "BillNye");
        _channel.Server_OnNick(null, args);

        Assert.IsNotNull(received, "NickChanged must fire when the renamed user is in the channel");
        Assert.AreEqual("BillNye", received.NewNick);
    }

    [Test]
    public void Server_OnNick_UserNotInChannel_DoesNotFireNickChanged()
    {
        _channel.Users = A.Fake<UserList>();
        A.CallTo(() => _channel.Users.GetUser(A<User>.Ignored)).Returns(null);

        bool fired = false;
        _channel.NickChanged += (_, _) => fired = true;

        _channel.Server_OnNick(null, new NickChangeEventArgs(new User("Stranger", "Stranger", ""), "BillNye"));

        Assert.IsFalse(fired, "NickChanged must not fire for a user that is not in this channel");
    }

    private static User MakeUser(string nick) => new User(nick, nick, "");

    [Test]
    public void UserJoin_NewUser_AddsToUserList()
    {
        var channel = CreateChannelWithConnection();

        channel.UserJoin(MakeUser("Bob"));

        Assert.IsNotNull(channel.Users.GetUser("Bob"),
            "UserJoin must add the user to the channel's user list");
    }

    [Test]
    public void UserJoin_SameNickTwice_NotAddedTwice()
    {
        var channel = CreateChannelWithConnection();

        channel.UserJoin(MakeUser("Bob"));
        channel.UserJoin(MakeUser("Bob"));

        Assert.AreEqual(1, channel.Users.Count,
            "UserJoin must not add a duplicate for a user already in the channel");
    }

    [Test]
    public void UserJoin_Always_FiresOnJoin()
    {
        var channel = CreateChannelWithConnection();
        bool onJoinFired = false;
        channel.OnJoin += (_, _) => onJoinFired = true;

        channel.UserJoin(MakeUser("Bob"));

        Assert.IsTrue(onJoinFired, "UserJoin must fire OnJoin");
    }

    [Test]
    public void RemoveUser_ExistingNick_RemovesUser()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        channel.RemoveUser("Bob");

        Assert.IsNull(channel.Users.GetUser("Bob"));
    }

    [Test]
    public void RemoveUser_UnknownNick_DoesNotThrowOrChangeList()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        Assert.DoesNotThrow(() => channel.RemoveUser("Nobody"));
        Assert.AreEqual(1, channel.Users.Count);
    }

    [Test]
    public void UserPart_OtherUser_RemovesUserAndFiresOtherUserParted()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));
        bool partedFired = false;
        channel.OtherUserParted += (_, _) => partedFired = true;

        channel.UserPart(MakeUser("Bob"), "bye");

        Assert.IsNull(channel.Users.GetUser("Bob"), "UserPart must remove the parting user");
        Assert.IsTrue(partedFired, "UserPart must fire OtherUserParted for a non-local user");
    }

    [Test]
    public void UserQuit_UserInChannel_RemovesUserAndFiresUserQuitted()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));
        bool quitFired = false;
        channel.UserQuitted += (_, _) => quitFired = true;

        channel.UserQuit(MakeUser("Bob"), "quit");

        Assert.IsNull(channel.Users.GetUser("Bob"), "UserQuit must remove the quitting user");
        Assert.IsTrue(quitFired, "UserQuit must fire UserQuitted when the user was in the channel");
    }

    [Test]
    public void UserQuit_UserNotInChannel_DoesNotFireUserQuitted()
    {
        var channel = CreateChannelWithConnection();
        bool quitFired = false;
        channel.UserQuitted += (_, _) => quitFired = true;

        channel.UserQuit(MakeUser("Ghost"), "quit");

        Assert.IsFalse(quitFired,
            "UserQuit must not fire UserQuitted for a user who is not in the channel");
    }

    [Test]
    public void UserKick_Always_RemovesKickeeAndFiresOnKick()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Victim"));
        bool kickFired = false;
        channel.OnKick += (_, _) => kickFired = true;

        channel.UserKick(MakeUser("Op"), "Victim", "rules");

        Assert.IsNull(channel.Users.GetUser("Victim"), "UserKick must remove the kicked user");
        Assert.IsTrue(kickFired, "UserKick must fire OnKick");
    }

    [Test]
    public void ApplyModeChanges_AddOperator_SetsAtPrefix()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Add, ChannelMode.ChannelOperator, '@', "Bob") });

        Assert.AreEqual('@', channel.Users.GetUser("Bob").Prefix);
    }

    [Test]
    public void ApplyModeChanges_AddVoice_SetsPlusPrefix()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Add, ChannelMode.Voice, '+', "Bob") });

        Assert.AreEqual('+', channel.Users.GetUser("Bob").Prefix);
    }

    [Test]
    public void ApplyModeChanges_OwnerFromPrefixToken_SetsTildePrefix()
    {
        // On Unreal, PREFIX=(qaohv)~&@%+ makes 'q' the owner mode. The letter maps to
        // ChannelMode.Quiet, so only the symbol from PREFIX tells the channel what it means.
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Add, ChannelMode.Quiet, '~', "Bob") });

        Assert.AreEqual('~', channel.Users.GetUser("Bob").Prefix);
    }

    [Test]
    public void ApplyModeChanges_SymbolUserCannotRank_IsIgnored()
    {
        // A server can announce a status symbol that User does not know. The mode must not
        // throw out of the Listener's event.
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        Assert.DoesNotThrow(() => channel.ApplyModeChanges(new[]
        {
            StatusMode(ModeAction.Add, ChannelMode.ChannelCreator, '!', "Bob")
        }));
        Assert.AreEqual('\0', channel.Users.GetUser("Bob").Prefix);
    }

    [Test]
    public void ApplyModeChanges_RemoveMatchingOperator_ClearsPrefix()
    {
        var channel = CreateChannelWithConnection();
        User bob = MakeUser("Bob");
        bob.Prefix = '@';
        channel.UserJoin(bob);

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Remove, ChannelMode.ChannelOperator, '@', "Bob") });

        Assert.AreEqual('\0', channel.Users.GetUser("Bob").Prefix);
    }

    [Test]
    public void ApplyModeChanges_UnknownUser_DoesNotThrow()
    {
        var channel = CreateChannelWithConnection();

        Assert.DoesNotThrow(() => channel.ApplyModeChanges(new[]
        {
            StatusMode(ModeAction.Add, ChannelMode.ChannelOperator, '@', "Nobody")
        }));
    }

    [Test]
    public void ApplyModeChanges_VoiceOnExistingOperator_StillShowsOperator()
    {
        var channel = CreateChannelWithConnection();
        User bob = MakeUser("Bob");
        bob.Prefix = '@';
        channel.UserJoin(bob);

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Add, ChannelMode.Voice, '+', "Bob") });

        User result = channel.Users.GetUser("Bob");
        Assert.AreEqual('@', result.Prefix,
            "An op who is also voiced must still display as op, not collapse to voice");
        Assert.IsTrue(result.HasStatus('+'), "Voice status must still be tracked alongside op");
    }

    [Test]
    public void ApplyModeChanges_RemoveOpFromOpAndVoiced_FallsBackToVoice()
    {
        var channel = CreateChannelWithConnection();
        User bob = MakeUser("Bob");
        bob.Prefix = '@';
        channel.UserJoin(bob);
        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Add, ChannelMode.Voice, '+', "Bob") });

        channel.ApplyModeChanges(new[] { StatusMode(ModeAction.Remove, ChannelMode.ChannelOperator, '@', "Bob") });

        Assert.AreEqual('+', channel.Users.GetUser("Bob").Prefix,
            "Removing op from an op+voiced user must fall back to the voice prefix");
    }

    [Test]
    public void ApplyModeChanges_NonStatusMode_LeavesPrefixUnchanged()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        channel.ApplyModeChanges(new[]
        {
            new ChannelModeInfo
            {
                Action = ModeAction.Add,
                Mode = ChannelMode.Ban,
                Parameter = "*!*@spam.host"
            }
        });

        Assert.AreEqual('\0', channel.Users.GetUser("Bob").Prefix,
            "A ban mode must not alter any member's op/voice prefix");
    }

    // The parser gives a status mode the symbol from the server's PREFIX token.
    private static ChannelModeInfo StatusMode(ModeAction action, ChannelMode mode, char symbol, string nick) =>
        new ChannelModeInfo { Action = action, Mode = mode, PrefixSymbol = symbol, Parameter = nick };

    private static ChannelModeInfo KeyMode(ModeAction action, string key) =>
        new ChannelModeInfo { Action = action, Mode = ChannelMode.Password, Parameter = key };

    [Test]
    public void ApplyModeChanges_AddKey_StoresIt()
    {
        var channel = CreateChannelWithConnection();

        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });

        Assert.AreEqual("hunter2", channel.Key);
    }

    [Test]
    public void ApplyModeChanges_RemoveKey_ClearsIt()
    {
        var channel = CreateChannelWithConnection();
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });

        // A -k names the key that is going away, so the parameter is still populated.
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Remove, "hunter2") });

        Assert.IsNull(channel.Key, "-k must clear the key rather than store the departing one");
    }

    [Test]
    public void ApplyModeChanges_KeyChange_FiresKeyChangedWithTheNewKey()
    {
        var channel = CreateChannelWithConnection();
        var announced = new List<string>();
        channel.KeyChanged += (_, e) => announced.Add(e.Data);

        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Remove, "hunter2") });

        CollectionAssert.AreEqual(new[] { "hunter2", null }, announced,
            "Setting and clearing the key must each announce the resulting key");
    }

    [Test]
    public void ApplyModeChanges_SameKeyAgain_DoesNotFireKeyChanged()
    {
        var channel = CreateChannelWithConnection();
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });
        int fired = 0;
        channel.KeyChanged += (_, _) => fired++;

        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });

        Assert.AreEqual(0, fired, "Re-reporting the key it already has is not a change");
    }

    [Test]
    public void ApplyModeChanges_KeyAlongsideStatusModes_AppliesBoth()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        // MODE #mutiny +ok Bob hunter2
        channel.ApplyModeChanges(new[]
        {
            StatusMode(ModeAction.Add, ChannelMode.ChannelOperator, '@', "Bob"),
            KeyMode(ModeAction.Add, "hunter2")
        });

        Assert.AreEqual("hunter2", channel.Key);
        Assert.AreEqual('@', channel.Users.GetUser("Bob").Prefix,
            "A key in the same MODE line must not stop the op prefix being applied");
    }

    [Test]
    public void ApplyModeSnapshot_WithoutAKey_ClearsAStaleOne()
    {
        var channel = CreateChannelWithConnection();
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });

        // RPL_CHANNELMODEIS reports every mode the channel has, and reports them all as adds.
        channel.ApplyModeSnapshot(new[]
        {
            new ChannelModeInfo
            {
                Action = ModeAction.Add,
                Mode = ChannelMode.TopicSettable,
                Parameter = ""
            }
        });

        Assert.IsNull(channel.Key,
            "A mode reply that names no key means the channel has none");
    }

    [Test]
    public void ApplyModeSnapshot_WithAKey_StoresIt()
    {
        var channel = CreateChannelWithConnection();

        channel.ApplyModeSnapshot(new[] { KeyMode(ModeAction.Add, "hunter2") });

        Assert.AreEqual("hunter2", channel.Key);
    }

    [Test]
    public void ApplyModeSnapshot_RepeatingTheKeyItHas_DoesNotFireKeyChanged()
    {
        var channel = CreateChannelWithConnection();
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });
        int fired = 0;
        channel.KeyChanged += (_, _) => fired++;

        // A bare /mode on a keyed channel replies with the key the channel already has. A
        // snapshot must not clear the key on its way to re-storing it: a listener that saw
        // the transient null would report the key as removed and then restored.
        channel.ApplyModeSnapshot(new[] { KeyMode(ModeAction.Add, "hunter2") });

        Assert.AreEqual(0, fired, "Re-reporting the key it already has is not a change");
    }

    [Test]
    public void ApplyModeSnapshot_ChangingTheKey_FiresKeyChangedOnceWithTheNewKey()
    {
        var channel = CreateChannelWithConnection();
        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });
        var announced = new List<string>();
        channel.KeyChanged += (_, e) => announced.Add(e.Data);

        channel.ApplyModeSnapshot(new[] { KeyMode(ModeAction.Add, "letmein") });

        CollectionAssert.AreEqual(new[] { "letmein" }, announced,
            "A new key must be announced once, with no null in between");
    }

    [Test]
    public void ApplyModeChanges_KeyOnly_DoesNotRefreshTheUserList()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));
        int refreshes = 0;
        channel.Users.Updated += (_, _) => refreshes++;

        channel.ApplyModeChanges(new[] { KeyMode(ModeAction.Add, "hunter2") });

        Assert.AreEqual(0, refreshes,
            "A mode line that names no member leaves the user list as it was");
    }

    [Test]
    public void PendingNames_InterleavedWithAnotherChannel_EachChannelGetsOnlyItsOwnMembers()
    {
        var chanA = CreateChannelWithConnection();
        var chanB = CreateChannelWithConnection();

        // Simulate the server pipelining NAMES runs for two channels:
        //   353 #a alice bob / 353 #b carol dave / 366 #a / 366 #b
        chanA.AddPendingNames(new[] { "alice", "bob" });
        chanB.AddPendingNames(new[] { "carol", "dave" });
        chanA.CommitPendingNames();
        chanB.CommitPendingNames();

        Assert.AreEqual(2, chanA.Users.Count);
        Assert.IsNotNull(chanA.Users.GetUser("alice"));
        Assert.IsNotNull(chanA.Users.GetUser("bob"));
        Assert.IsNull(chanA.Users.GetUser("carol"), "channel A must not be contaminated by channel B's NAMES");
        Assert.IsNull(chanA.Users.GetUser("dave"));

        Assert.AreEqual(2, chanB.Users.Count);
        Assert.IsNotNull(chanB.Users.GetUser("carol"));
        Assert.IsNotNull(chanB.Users.GetUser("dave"),
            "channel B must still be seeded even though channel A committed first");
    }

    [Test]
    public void CommitPendingNames_ClearsBuffer_SoNextSeedIsIndependent()
    {
        var channel = CreateChannelWithConnection();

        channel.AddPendingNames(new[] { "alice" });
        channel.CommitPendingNames();

        channel.AddPendingNames(new[] { "bob" });
        channel.CommitPendingNames();

        Assert.AreEqual(1, channel.Users.Count,
            "A committed NAMES buffer must be cleared so a later NAMES does not accumulate stale users");
        Assert.IsNotNull(channel.Users.GetUser("bob"));
        Assert.IsNull(channel.Users.GetUser("alice"));
    }

    [Test]
    public void Membership_NewChannel_DefaultsToNotJoined()
    {
        var channel = CreateChannelWithConnection();

        Assert.AreEqual(ChannelMembership.NotJoined, channel.Membership);
        Assert.IsFalse(channel.Joined, "A freshly created channel must not report as joined");
    }

    [Test]
    public void Joined_MembershipJoinedButNoUsers_ReturnsTrue()
    {
        var channel = CreateChannelWithConnection();
        channel.Membership = ChannelMembership.Joined;

        // Regression: between the JOIN echo and the NAMES reply the user list is
        // empty, but we are still a member. Joined must not depend on Users.Count.
        Assert.IsTrue(channel.Joined,
            "Joined must reflect membership state, not whether the user list is populated");
    }

    [Test]
    public void Joined_MembershipNotJoinedButHasUsers_ReturnsFalse()
    {
        var channel = CreateChannelWithConnection();
        channel.UserJoin(MakeUser("Bob"));

        Assert.IsFalse(channel.Joined,
            "A populated user list must not make a non-member channel report as joined");
    }

    [Test]
    public void Joined_MembershipJoining_ReturnsFalse()
    {
        var channel = CreateChannelWithConnection();
        channel.Membership = ChannelMembership.Joining;

        Assert.IsFalse(channel.Joined,
            "A channel awaiting its JOIN echo is not yet joined");
    }

    [Test]
    public void AddPendingNames_StripsStatusPrefixesIntoUserStatus()
    {
        var channel = CreateChannelWithConnection();

        channel.AddPendingNames(new[] { "@alice", "+bob", "charlie" });
        channel.CommitPendingNames();

        Assert.AreEqual('@', channel.Users.GetUser("alice").Prefix);
        Assert.AreEqual('+', channel.Users.GetUser("bob").Prefix);
        Assert.AreEqual('\0', channel.Users.GetUser("charlie").Prefix);
    }
}
