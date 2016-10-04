using NUnit.Framework;
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
