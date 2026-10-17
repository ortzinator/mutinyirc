using System.Linq;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class QueryCommandTests
{
    private ISender _fakeSender;
    private Server _server;
    private Channel _channel;
    private CommandHarness _harness;

    [SetUp]
    public void Setup()
    {
        _fakeSender = A.Fake<ISender>();
        var fakeConn = A.Fake<IConnection>();
        A.CallTo(() => fakeConn.Sender).Returns(_fakeSender);
        _server = A.Fake<Server>();
        A.CallTo(() => _server.Connection).Returns(fakeConn);
        _channel = new Channel(_server, "#mutiny");

        _harness = CommandHarness.For<Query>("query");
    }

    [Test]
    public void NickOnly_OpensTabWithoutSending()
    {
        CommandResultInfo result = _harness.Dispatch(_channel, "someone");

        Assert.That(result.Result, Is.EqualTo(Result.Success));
        Assert.That(_server.PMSessions.Any(s => s.User.Nick == "someone"), Is.True);
        A.CallTo(() => _fakeSender.PrivateMessage(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public void NickWithMessage_OpensTabAndSends()
    {
        CommandResultInfo result = _harness.Dispatch(_channel, "someone", "hello", "there");

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

        _harness.Dispatch(_channel, "someone", "hi");

        Assert.That(echoed, Is.EqualTo("hi"));
    }

    [Test]
    public void ServiceNick_FailsAndSendsNothing()
    {
        _server.ServiceNicks.Add("NickServ");

        CommandResultInfo result = _harness.Dispatch(_channel, "NickServ", "identify", "secret");

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        A.CallTo(() => _fakeSender.PrivateMessage(A<string>._, A<string>._)).MustNotHaveHappened();
    }
}
