using NUnit.Framework;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using FakeItEasy;
using FlamingIRC;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class ModeCommandTests
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

        // --- Current channel, no explicit target ---

        [Test]
        public void Execute_NoArgs_RequestsCurrentChannelModes()
        {
            new Mode().Execute(_channel);

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny")).MustHaveHappened();
        }

        [Test]
        public void Execute_PlusFlagOnly_SetsModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, "+i");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny +i")).MustHaveHappened();
        }

        [Test]
        public void Execute_MinusFlagOnly_RemovesModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, new[] { 'i' });

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny -i")).MustHaveHappened();
        }

        [Test]
        public void Execute_PlusFlagWithParam_SetsModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, "+o", "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny +o nick")).MustHaveHappened();
        }

        [Test]
        public void Execute_MultiPlusFlagWithParams_SetsModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, "+ooo", "a b c");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny +ooo a b c")).MustHaveHappened();
        }

        [Test]
        public void Execute_MinusFlagWithParam_RemovesModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, new[] { 'o' }, "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny -o nick")).MustHaveHappened();
        }

        [Test]
        public void Execute_MultiMinusFlagWithParams_RemovesModeOnCurrentChannel()
        {
            new Mode().Execute(_channel, new[] { 'o', 'o', 'o' }, "a b c");

            A.CallTo(() => _fakeSender.Raw("MODE #mutiny -ooo a b c")).MustHaveHappened();
        }

        // --- Explicit nick target ---

        [Test]
        public void Execute_BareTarget_RequestsThatTargetsModes()
        {
            new Mode().Execute(_channel, "someone");

            A.CallTo(() => _fakeSender.Raw("MODE someone")).MustHaveHappened();
        }

        [Test]
        public void Execute_NickWithPlusFlag_SetsUserMode()
        {
            new Mode().Execute(_channel, "someone", "+i");

            A.CallTo(() => _fakeSender.Raw("MODE someone +i")).MustHaveHappened();
        }

        [Test]
        public void Execute_NickWithMinusFlag_RemovesUserMode()
        {
            new Mode().Execute(_channel, "someone", new[] { 'i' });

            A.CallTo(() => _fakeSender.Raw("MODE someone -i")).MustHaveHappened();
        }

        [Test]
        public void Execute_NickWithMinusFlagAndParam_RemovesUserMode()
        {
            new Mode().Execute(_channel, "someone", new[] { 'o' }, "x");

            A.CallTo(() => _fakeSender.Raw("MODE someone -o x")).MustHaveHappened();
        }

        // --- Explicit channel target ---

        [Test]
        public void Execute_ExplicitChannel_RequestsThatChannelsModes()
        {
            new Mode().Execute(_channel, new ChannelInfo("#other"));

            A.CallTo(() => _fakeSender.Raw("MODE #other")).MustHaveHappened();
        }

        [Test]
        public void Execute_ExplicitChannelWithPlusSpec_SetsMode()
        {
            new Mode().Execute(_channel, new ChannelInfo("#other"), "+o nick");

            A.CallTo(() => _fakeSender.Raw("MODE #other +o nick")).MustHaveHappened();
        }

        [Test]
        public void Execute_ExplicitChannelWithMinusFlag_RemovesMode()
        {
            new Mode().Execute(_channel, new ChannelInfo("#other"), new[] { 'i' });

            A.CallTo(() => _fakeSender.Raw("MODE #other -i")).MustHaveHappened();
        }

        [Test]
        public void Execute_ExplicitChannelWithMinusFlagAndParam_RemovesMode()
        {
            new Mode().Execute(_channel, new ChannelInfo("#other"), new[] { 'o' }, "nick");

            A.CallTo(() => _fakeSender.Raw("MODE #other -o nick")).MustHaveHappened();
        }
    }
}
