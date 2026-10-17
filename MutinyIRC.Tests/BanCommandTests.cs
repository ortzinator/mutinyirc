using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class BanCommandTests
{
    private ISender _fakeSender;
    private Server _server;
    private Channel _channel;
    private PrivateMessageSession _pm;
    private CommandHarness _harness;

    [SetUp]
    public void Setup()
    {
        _fakeSender = A.Fake<ISender>();
        var fakeConn = A.Fake<IConnection>();
        A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
        A.CallTo(() => fakeConn.ConnectionData).Returns(new ConnectionArgs { Nick = "me" });
        _server = new Server { Connection = fakeConn };

        _channel = new Channel(_server, "#mutiny");
        _channel.Users.Add(new User("bob", "~bob", "dsl.example.net"));
        _channel.Users.Add(new User("ghost", "", ""));   // host unknown (NAMES-only)
        _server.Channels.Add(_channel.Name, _channel);

        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

        _harness = CommandHarness.For<Ban>("ban");
    }

    [Test]
    public void BanByNick_HostKnown_UsesWildcardHostMask()
    {
        _harness.Dispatch(_channel, "bob");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByNick_HostUnknown_FallsBackToNickMask()
    {
        _harness.Dispatch(_channel, "ghost");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "ghost!*@*"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByNick_NotInChannel_FallsBackToNickMask()
    {
        _harness.Dispatch(_channel, "stranger");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "stranger!*@*"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByLiteralMask_PassesThroughVerbatim()
    {
        _harness.Dispatch(_channel, "*!*@evil.host");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@evil.host"))
            .MustHaveHappened();
    }

    [Test]
    public void KickBan_BansThenKicks()
    {
        _harness.Dispatch(_channel, "-k", "bob", "spamming");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
        A.CallTo(() => _fakeSender.Kick("#mutiny", "spamming", "bob")).MustHaveHappened();
    }

    [Test]
    public void KickBan_NoReason_DefaultsToOwnNick()
    {
        _harness.Dispatch(_channel, "-k", "bob");

        A.CallTo(() => _fakeSender.Kick("#mutiny", "me", "bob")).MustHaveHappened();
    }

    [Test]
    public void RemoveBan_SendsMinusB_NoKick()
    {
        _harness.Dispatch(_channel, "-r", "*!*@evil.host");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Remove, ChannelMode.Ban, "*!*@evil.host"))
            .MustHaveHappened();
        A.CallTo(() => _fakeSender.Kick(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void KickAndRemoveTogether_Fails()
    {
        CommandResultInfo result = _harness.Dispatch(_channel, "-kr", "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void ExplicitChannel_FromPrivateMessage_Bans()
    {
        _harness.Dispatch(_pm, "#mutiny", "bob");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
    }

    [Test]
    public void FromPrivateMessage_NoChannel_Fails()
    {
        CommandResultInfo result = _harness.Dispatch(_pm, "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void UntrackedChannel_Fails()
    {
        CommandResultInfo result = _harness.Dispatch(_pm, "#nowhere", "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void MissingTarget_Fails()
    {
        CommandResultInfo result = _harness.Dispatch(_channel, "-k");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
    }
}
