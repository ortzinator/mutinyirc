using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class AwayCommandTests
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
        // A real Server (not a fake) so its runtime type matches the dispatcher's exact
        // GetType() check for the Server-context overload.
        _server = new Server { Connection = fakeConn };
        _channel = new Channel(_server, "#mutiny");
        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

        _harness = CommandHarness.For<Away>("away");
    }

    [Test]
    public void ServerContext_WithMessage_SetsAway()
    {
        _harness.Dispatch(_server, "Gone", "fishing");

        A.CallTo(() => _fakeSender.Away("Gone fishing")).MustHaveHappened();
    }

    [Test]
    public void ChannelContext_WithMessage_RoutesToServer()
    {
        _harness.Dispatch(_channel, "brb");

        A.CallTo(() => _fakeSender.Away("brb")).MustHaveHappened();
    }

    [Test]
    public void PrivateMessageContext_WithMessage_RoutesToServer()
    {
        _harness.Dispatch(_pm, "lunch");

        A.CallTo(() => _fakeSender.Away("lunch")).MustHaveHappened();
    }

    [Test] // bare /away must clear, not call Away("") which the protocol layer rejects
    public void NoArguments_ClearsAway()
    {
        _harness.Dispatch(_server);

        A.CallTo(() => _fakeSender.UnAway()).MustHaveHappened();
        A.CallTo(() => _fakeSender.Away(A<string>._)).MustNotHaveHappened();
    }

    [Test] // whitespace-only tail is treated the same as bare /away
    public void WhitespaceOnly_ClearsAway()
    {
        _harness.Dispatch(_server, "   ");

        A.CallTo(() => _fakeSender.UnAway()).MustHaveHappened();
        A.CallTo(() => _fakeSender.Away(A<string>._)).MustNotHaveHappened();
    }
}
