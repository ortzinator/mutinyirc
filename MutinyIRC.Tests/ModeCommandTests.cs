using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class ModeCommandTests
{
    private ISender _fakeSender;
    private Channel _channel;
    private CommandHarness _harness;

    [SetUp]
    public void Setup()
    {
        _fakeSender = A.Fake<ISender>();
        var fakeConn = A.Fake<IConnection>();
        A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
        var server = A.Fake<Server>();
        A.CallTo(() => server.Connection).Returns(fakeConn);
        _channel = new Channel(server, "#mutiny");

        _harness = CommandHarness.For<Mode>("mode");
    }

    [Test]
    public void BareMode_RequestsCurrentChannelModes()
    {
        _harness.Dispatch(_channel);

        A.CallTo(() => _fakeSender.Raw("MODE #mutiny")).MustHaveHappened();
    }

    [Test]
    public void PlusSpec_AppliesToCurrentChannel()
    {
        _harness.Dispatch(_channel, "+o", "nick");

        A.CallTo(() => _fakeSender.Raw("MODE #mutiny +o nick")).MustHaveHappened();
    }

    [Test] // the case that previously forced char[] coercion + Removal reconstruction
    public void MinusSpec_PassesThroughVerbatim()
    {
        _harness.Dispatch(_channel, "-ooo", "a", "b", "c");

        A.CallTo(() => _fakeSender.Raw("MODE #mutiny -ooo a b c")).MustHaveHappened();
    }

    [Test]
    public void PlusFlagOnly_SetsModeOnCurrentChannel()
    {
        _harness.Dispatch(_channel, "+i");

        A.CallTo(() => _fakeSender.Raw("MODE #mutiny +i")).MustHaveHappened();
    }

    [Test]
    public void MinusFlagOnly_RemovesModeOnCurrentChannel()
    {
        _harness.Dispatch(_channel, "-i");

        A.CallTo(() => _fakeSender.Raw("MODE #mutiny -i")).MustHaveHappened();
    }

    [Test]
    public void ExplicitChannel_RequestsThatChannelsModes()
    {
        _harness.Dispatch(_channel, "#other");

        A.CallTo(() => _fakeSender.Raw("MODE #other")).MustHaveHappened();
    }

    [Test]
    public void ExplicitChannel_WithPlusSpec()
    {
        _harness.Dispatch(_channel, "#other", "+o", "nick");

        A.CallTo(() => _fakeSender.Raw("MODE #other +o nick")).MustHaveHappened();
    }

    [Test]
    public void ExplicitChannel_WithRemoval()
    {
        _harness.Dispatch(_channel, "#other", "-o", "nick");

        A.CallTo(() => _fakeSender.Raw("MODE #other -o nick")).MustHaveHappened();
    }

    [Test]
    public void NickTarget_RequestsThatNicksModes()
    {
        _harness.Dispatch(_channel, "someone");

        A.CallTo(() => _fakeSender.Raw("MODE someone")).MustHaveHappened();
    }

    [Test]
    public void NickTarget_WithUserMode()
    {
        _harness.Dispatch(_channel, "someone", "+i");

        A.CallTo(() => _fakeSender.Raw("MODE someone +i")).MustHaveHappened();
    }
}
