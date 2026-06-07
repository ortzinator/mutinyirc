namespace MutinyIRC.Tests
{
    using System.Collections.Generic;
    using FakeItEasy;
    using FlamingIRC;
    using MutinyIRC.Commands;
    using MutinyIRC.Common;
    using MutinyIRC.PluginFramework;
    using NUnit.Framework;

    [TestFixture]
    public class NoticeCommandTests
    {
        private ISender _fakeSender;
        private Server _server;
        private Channel _channel;
        private PluginManager _manager;

        [SetUp]
        public void Setup()
        {
            _fakeSender = A.Fake<ISender>();
            var fakeConn = A.Fake<IConnection>();
            A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
            _server = A.Fake<Server>();
            A.CallTo(() => _server.Connection).Returns(fakeConn);
            _channel = new Channel(_server, "#mutiny");

            _manager = new PluginManager();
            var type = typeof(Notice);
            _manager._commands.Add(type.FullName,
                new CommandInfo(type.Assembly.Location, type.FullName, "notice", typeof(ICommand)));
        }

        private CommandResultInfo Dispatch(MessageContext context, params string[] args)
            => _manager.ExecuteCommand(new CommandExecutionInfo
            {
                Name = "notice",
                Context = context,
                ParameterList = new List<object>(args),
            });

        [Test]
        public void NickTarget_SendsPrivateNotice()
        {
            Dispatch(_channel, "someone", "hello", "there");

            A.CallTo(() => _fakeSender.PrivateNotice("someone", "hello there")).MustHaveHappened();
        }

        [Test]
        public void ChannelTarget_SendsPublicNotice()
        {
            Dispatch(_channel, "#other", "heads", "up");

            A.CallTo(() => _fakeSender.PublicNotice("#other", "heads up")).MustHaveHappened();
        }

        [Test]
        public void EchoesOnInvokingContext_NotServer()
        {
            UserMessageEventArgs captured = null;
            _channel.NoticeSent += (_, e) => captured = e;

            Dispatch(_channel, "someone", "hi");

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.User.Nick, Is.EqualTo("someone"));
            Assert.That(captured.Message, Is.EqualTo("hi"));
        }

        [Test]
        public void MissingMessage_FailsWithUsage()
        {
            CommandResultInfo result = Dispatch(_channel, "someone");

            Assert.That(result.Result, Is.EqualTo(Result.Fail));
            A.CallTo(() => _fakeSender.PrivateNotice(A<string>._, A<string>._)).MustNotHaveHappened();
        }
    }
}
