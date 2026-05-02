using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using OrtzIRC.Common;
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
            _channel.TopicReceived += delegate (object sender, OrtzIRC.Common.DataEventArgs<string> e)
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
    }
}
