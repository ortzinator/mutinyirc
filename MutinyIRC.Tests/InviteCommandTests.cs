using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class InviteCommandTests
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
        _server = new Server { Connection = fakeConn };
        _channel = new Channel(_server, "#mutiny");
        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

        _harness = CommandHarness.For<Invite>("invite");
    }

    [Test]
    public void NickAndChannel_SendsInvite()
    {
        _harness.Dispatch(_channel, "someone", "#other");

        A.CallTo(() => _fakeSender.Invite("someone", "#other")).MustHaveHappened();
    }

    [Test]
    public void NickOnly_FromChannel_InvitesToCurrentChannel()
    {
        _harness.Dispatch(_channel, "someone");

        A.CallTo(() => _fakeSender.Invite("someone", "#mutiny")).MustHaveHappened();
    }

    [Test]
    public void NickAndChannel_FromServer_SendsInvite()
    {
        _harness.Dispatch(_server, "someone", "#other");

        A.CallTo(() => _fakeSender.Invite("someone", "#other")).MustHaveHappened();
    }

    [Test]
    public void NickAndChannel_FromPrivateMessage_SendsInvite()
    {
        _harness.Dispatch(_pm, "someone", "#other");

        A.CallTo(() => _fakeSender.Invite("someone", "#other")).MustHaveHappened();
    }
}
