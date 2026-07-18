using System.Collections.Generic;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class BanCommandTests
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
        A.CallTo(() => fakeConn.ConnectionData).Returns(new ConnectionArgs { Nick = "me" });
        _server = new Server { Connection = fakeConn };

        _channel = new Channel(_server, "#mutiny");
        _channel.Users.Add(new User("bob", "~bob", "dsl.example.net"));
        _channel.Users.Add(new User("ghost", "", ""));   // host unknown (NAMES-only)
        _server.Channels.Add(_channel.Name, _channel);

        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

        _manager = new PluginManager();
        var type = typeof(Ban);
        _manager._commands.Add(type.FullName,
            new CommandInfo(type.Assembly.Location, type.FullName, "ban", typeof(ICommand)));
    }

    private CommandResultInfo Dispatch(MessageContext context, params string[] args)
        => _manager.ExecuteCommand(new CommandExecutionInfo
        {
            Name = "ban",
            Context = context,
            ParameterList = new List<object>(args),
        });

    [Test]
    public void BanByNick_HostKnown_UsesWildcardHostMask()
    {
        Dispatch(_channel, "bob");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByNick_HostUnknown_FallsBackToNickMask()
    {
        Dispatch(_channel, "ghost");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "ghost!*@*"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByNick_NotInChannel_FallsBackToNickMask()
    {
        Dispatch(_channel, "stranger");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "stranger!*@*"))
            .MustHaveHappened();
    }

    [Test]
    public void BanByLiteralMask_PassesThroughVerbatim()
    {
        Dispatch(_channel, "*!*@evil.host");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@evil.host"))
            .MustHaveHappened();
    }

    [Test]
    public void KickBan_BansThenKicks()
    {
        Dispatch(_channel, "-k", "bob", "spamming");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
        A.CallTo(() => _fakeSender.Kick("#mutiny", "spamming", "bob")).MustHaveHappened();
    }

    [Test]
    public void KickBan_NoReason_DefaultsToOwnNick()
    {
        Dispatch(_channel, "-k", "bob");

        A.CallTo(() => _fakeSender.Kick("#mutiny", "me", "bob")).MustHaveHappened();
    }

    [Test]
    public void RemoveBan_SendsMinusB_NoKick()
    {
        Dispatch(_channel, "-r", "*!*@evil.host");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Remove, ChannelMode.Ban, "*!*@evil.host"))
            .MustHaveHappened();
        A.CallTo(() => _fakeSender.Kick(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void KickAndRemoveTogether_Fails()
    {
        CommandResultInfo result = Dispatch(_channel, "-kr", "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void ExplicitChannel_FromPrivateMessage_Bans()
    {
        Dispatch(_pm, "#mutiny", "bob");

        A.CallTo(() => _fakeSender.ChangeChannelMode(
            "#mutiny", ModeAction.Add, ChannelMode.Ban, "*!*@dsl.example.net"))
            .MustHaveHappened();
    }

    [Test]
    public void FromPrivateMessage_NoChannel_Fails()
    {
        CommandResultInfo result = Dispatch(_pm, "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void UntrackedChannel_Fails()
    {
        CommandResultInfo result = Dispatch(_pm, "#nowhere", "bob");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.ChangeChannelMode(
            A<string>._, A<ModeAction>._, A<ChannelMode>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void MissingTarget_Fails()
    {
        CommandResultInfo result = Dispatch(_channel, "-k");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
    }
}
