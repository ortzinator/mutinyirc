using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class QueryCommandTests
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
        var type = typeof(Query);
        _manager._commands.Add(type.FullName,
            new CommandInfo(type.Assembly.Location, type.FullName, "query", typeof(ICommand)));
    }

    private CommandResultInfo Dispatch(MessageContext context, params string[] args)
        => _manager.ExecuteCommand(new CommandExecutionInfo
        {
            Name = "query",
            Context = context,
            ParameterList = new List<object>(args),
        });

    [Test]
    public void NickOnly_OpensTabWithoutSending()
    {
        CommandResultInfo result = Dispatch(_channel, "someone");

        Assert.That(result.Result, Is.EqualTo(Result.Success));
        Assert.That(_server.PMSessions.Any(s => s.User.Nick == "someone"), Is.True);
        A.CallTo(() => _fakeSender.PrivateMessage(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public void NickWithMessage_OpensTabAndSends()
    {
        CommandResultInfo result = Dispatch(_channel, "someone", "hello", "there");

        Assert.That(result.Result, Is.EqualTo(Result.Success));
        Assert.That(_server.PMSessions.Any(s => s.User.Nick == "someone"), Is.True);
        A.CallTo(() => _fakeSender.PrivateMessage("someone", "hello there")).MustHaveHappened();
    }

    [Test]
    public void NickWithMessage_EchoesOnSession()
    {
        string echoed = null;
        _server.PrivateMessageSessionAdded += (_, e) =>
            e.PrivateMessageSession.MessageSent += (_, args) => echoed = args.Data;

        Dispatch(_channel, "someone", "hi");

        Assert.That(echoed, Is.EqualTo("hi"));
    }

    [Test]
    public void ServiceNick_FailsAndSendsNothing()
    {
        _server.ServiceNicks.Add("NickServ");

        CommandResultInfo result = Dispatch(_channel, "NickServ", "identify", "secret");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.PrivateMessage(A<string>._, A<string>._)).MustNotHaveHappened();
    }
}
