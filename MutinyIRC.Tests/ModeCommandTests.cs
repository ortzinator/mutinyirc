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
    public class ModeCommandTests
    {
        private ISender _fakeSender;
        private Channel _channel;
        private PluginManager _manager;

        [SetUp]
        public void Setup()
        {
            _fakeSender = A.Fake<ISender>();
            var fakeConn = A.Fake<IConnection>();
            A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
            var server = A.Fake<Server>();
            A.CallTo(() => server.Connection).Returns(fakeConn);
            _channel = new Channel(server, "#mutiny");

            _manager = new PluginManager();
            var type = typeof(Mode);
            _manager._commands.Add(type.FullName,
                new CommandInfo(type.Assembly.Location, type.FullName, "mode", typeof(ICommand)));
        }

        // Dispatches /mode through the real PluginManager so the coercion path (or, here, the
        // [RawArguments] opt-out) is exercised — not just the command body in isolation.
        private void Dispatch(params string[] args)
            => _manager.ExecuteCommand(new CommandExecutionInfo
            {
                Name = "mode",
                Context = _channel,
                ParameterList = new List<object>(args),
            });

        [Test]
        public void BareMode_RequestsCurrentChannelModes()
        {
            Dispatch();

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny")).MustHaveHappened();
        }

        [Test]
        public void PlusSpec_AppliesToCurrentChannel()
        {
            Dispatch("+o", "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny +o nick")).MustHaveHappened();
        }

        [Test] // the case that previously forced char[] coercion + Removal reconstruction
        public void MinusSpec_PassesThroughVerbatim()
        {
            Dispatch("-ooo", "a", "b", "c");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny -ooo a b c")).MustHaveHappened();
        }

        [Test]
        public void PlusFlagOnly_SetsModeOnCurrentChannel()
        {
            Dispatch("+i");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny +i")).MustHaveHappened();
        }

        [Test]
        public void MinusFlagOnly_RemovesModeOnCurrentChannel()
        {
            Dispatch("-i");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny -i")).MustHaveHappened();
        }

        [Test]
        public void ExplicitChannel_RequestsThatChannelsModes()
        {
            Dispatch("#other");

            A.CallTo(() => _fakeSender.Raw("MODE #other")).MustHaveHappened();
        }

        [Test]
        public void ExplicitChannel_WithPlusSpec()
        {
            Dispatch("#other", "+o", "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #other +o nick")).MustHaveHappened();
        }

        [Test]
        public void ExplicitChannel_WithRemoval()
        {
            Dispatch("#other", "-o", "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #other -o nick")).MustHaveHappened();
        }

        [Test]
        public void NickTarget_RequestsThatNicksModes()
        {
            Dispatch("someone");

            A.CallTo(() => _fakeSender.Raw("MODE someone")).MustHaveHappened();
        }

        [Test]
        public void NickTarget_WithUserMode()
        {
            Dispatch("someone", "+i");

            A.CallTo(() => _fakeSender.Raw("MODE someone +i")).MustHaveHappened();
        }
    }
}
