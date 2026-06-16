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
    public class InviteCommandTests
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
            var type = typeof(Invite);
            _manager._commands.Add(type.FullName,
                new CommandInfo(type.Assembly.Location, type.FullName, "invite", typeof(ICommand)));
        }

        private CommandResultInfo Dispatch(MessageContext context, params string[] args)
            => _manager.ExecuteCommand(new CommandExecutionInfo
            {
                Name = "invite",
                Context = context,
                ParameterList = new List<object>(args),
            });

        [Test]
        public void NickAndChannel_SendsInvite()
        {
            Dispatch(_channel, "someone", "#other");

            A.CallTo(() => _fakeSender.Invite("someone", "#other")).MustHaveHappened();
        }

        [Test]
        public void NickOnly_FromChannel_InvitesToCurrentChannel()
        {
            Dispatch(_channel, "someone");

            A.CallTo(() => _fakeSender.Invite("someone", "#mutiny")).MustHaveHappened();
        }

        [Test]
        public void NickAndChannel_FromServer_SendsInvite()
        {
            new Invite().Execute(_server, "someone", new ChannelInfo("#other"));

            A.CallTo(() => _fakeSender.Invite("someone", "#other")).MustHaveHappened();
        }
    }
}