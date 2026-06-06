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
    public class RawCommandTests
    {
        private ISender _fakeSender;
        private Server _server;
        private Channel _channel;
        private PrivateMessageSession _pm;
        private PluginManager _manager;

        [SetUp]
        public void Setup()
        {
            _fakeSender = A.Fake<ISender>();
            var fakeConn = A.Fake<IConnection>();
            A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
            // A real Server (not a fake) so its runtime type matches the dispatcher's exact
            // GetType() check for the Server-context overload.
            _server = new Server { Connection = fakeConn };
            _channel = new Channel(_server, "#mutiny");
            _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

            _manager = new PluginManager();
            var type = typeof(Raw);
            _manager._commands.Add(type.FullName,
                new CommandInfo(type.Assembly.Location, type.FullName, "raw", typeof(ICommand)));
        }

        // Dispatches /raw through the real PluginManager so the [RawArguments] opt-out is exercised.
        private void Dispatch(MessageContext context, params string[] args)
            => _manager.ExecuteCommand(new CommandExecutionInfo
            {
                Name = "raw",
                Context = context,
                ParameterList = new List<object>(args),
            });

        [Test]
        public void ServerContext_ForwardsVerbatim()
        {
            Dispatch(_server, "WHOIS", "someone");

            A.CallTo(() => _fakeSender.Raw("WHOIS someone")).MustHaveHappened();
        }

        [Test]
        public void ChannelContext_RoutesToServer()
        {
            Dispatch(_channel, "JOIN", "#channel");

            A.CallTo(() => _fakeSender.Raw("JOIN #channel")).MustHaveHappened();
        }

        [Test]
        public void PrivateMessageContext_RoutesToServer()
        {
            Dispatch(_pm, "PRIVMSG", "#chan", ":hello");

            A.CallTo(() => _fakeSender.Raw("PRIVMSG #chan :hello")).MustHaveHappened();
        }

        [Test] // colons, spaces, and a leading + must survive untouched
        public void PreservesColonsAndLeadingPlus()
        {
            Dispatch(_channel, "PRIVMSG", "#chan", ":+ohai");

            A.CallTo(() => _fakeSender.Raw("PRIVMSG #chan :+ohai")).MustHaveHappened();
        }

        [Test]
        public void EmptyArguments_SendsNothing()
        {
            Dispatch(_server);

            A.CallTo(() => _fakeSender.Raw(A<string>._)).MustNotHaveHappened();
        }
    }
}