using NUnit.Framework;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using FakeItEasy;
using FlamingIRC;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class TopicCommandTests
    {
        private ISender _fakeSender;
        private Channel _channel;

        [SetUp]
        public void Setup()
        {
            _fakeSender = A.Fake<ISender>();
            var fakeConn = A.Fake<IConnection>();
            A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
            var server = A.Fake<Server>();
            A.CallTo(() => server.Connection).Returns(fakeConn);
            _channel = new Channel(server, "#mutiny");
        }

        [Test]
        public void Execute_NoTopic_RequestsCurrentTopic()
        {
            new Topic().Execute(_channel);

            A.CallTo(() => _fakeSender.RequestTopic("#mutiny")).MustHaveHappened();
        }

        [Test]
        public void Execute_WithTopic_ChangesTopic()
        {
            new Topic().Execute(_channel, "new topic");

            A.CallTo(() => _fakeSender.ChangeTopic("#mutiny", "new topic")).MustHaveHappened();
        }
    }
}
