using NUnit.Framework;
using OrtzIRC.Common;
using FakeItEasy;

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
            _channel.TopicReceived += (sender, args) => eventWasRaised = true;
            _channel.ShowTopic("Topic here and stuff");
            Assert.IsTrue(eventWasRaised, "TopicRecieved event was not fired");
        }
    }
}
