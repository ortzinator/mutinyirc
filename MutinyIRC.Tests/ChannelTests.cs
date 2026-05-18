using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;
using FakeItEasy;
using FlamingIRC;

namespace MutinyIRC.Tests
{
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
        public void ShowTopic_TopicRecievedRegistered_EventFires()
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
            Assert.IsTrue(eventWasRaised, "TopicRecieved event was not fired");
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
            channel.OnKick += (_, _, _) => kickFired = true;

            channel.UserKick(MakeUser("Op"), "Victim", "rules");

            Assert.IsNull(channel.Users.GetUser("Victim"), "UserKick must remove the kicked user");
            Assert.IsTrue(kickFired, "UserKick must fire OnKick");
        }

        [Test]
        public void ApplyModeChanges_AddOperator_SetsAtPrefix()
        {
            var channel = CreateChannelWithConnection();
            channel.UserJoin(MakeUser("Bob"));

            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Add,
                    Mode = ChannelMode.ChannelOperator,
                    Parameter = "Bob"
                }
            });

            Assert.AreEqual('@', channel.Users.GetUser("Bob").Prefix);
        }

        [Test]
        public void ApplyModeChanges_AddVoice_SetsPlusPrefix()
        {
            var channel = CreateChannelWithConnection();
            channel.UserJoin(MakeUser("Bob"));

            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Add,
                    Mode = ChannelMode.Voice,
                    Parameter = "Bob"
                }
            });

            Assert.AreEqual('+', channel.Users.GetUser("Bob").Prefix);
        }

        [Test]
        public void ApplyModeChanges_RemoveMatchingOperator_ClearsPrefix()
        {
            var channel = CreateChannelWithConnection();
            User bob = MakeUser("Bob");
            bob.Prefix = '@';
            channel.UserJoin(bob);

            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Remove,
                    Mode = ChannelMode.ChannelOperator,
                    Parameter = "Bob"
                }
            });

            Assert.AreEqual('\0', channel.Users.GetUser("Bob").Prefix);
        }

        [Test]
        public void ApplyModeChanges_UnknownUser_DoesNotThrow()
        {
            var channel = CreateChannelWithConnection();

            Assert.DoesNotThrow(() => channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Add,
                    Mode = ChannelMode.ChannelOperator,
                    Parameter = "Nobody"
                }
            }));
        }

        [Test]
        public void ApplyModeChanges_VoiceOnExistingOperator_StillShowsOperator()
        {
            var channel = CreateChannelWithConnection();
            User bob = MakeUser("Bob");
            bob.Prefix = '@';
            channel.UserJoin(bob);

            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Add,
                    Mode = ChannelMode.Voice,
                    Parameter = "Bob"
                }
            });

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
            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo { Action = ModeAction.Add, Mode = ChannelMode.Voice, Parameter = "Bob" }
            });

            channel.ApplyModeChanges(new[]
            {
                new ChannelModeInfo
                {
                    Action = ModeAction.Remove,
                    Mode = ChannelMode.ChannelOperator,
                    Parameter = "Bob"
                }
            });

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
    }
}
