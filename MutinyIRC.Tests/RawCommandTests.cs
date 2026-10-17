using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class RawCommandTests
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

        _harness = CommandHarness.For<Raw>("raw");
    }

    [Test]
    public void ServerContext_ForwardsVerbatim()
    {
        _harness.Dispatch(_server, "WHOIS", "someone");

        A.CallTo(() => _fakeSender.Raw("WHOIS someone")).MustHaveHappened();
    }

    [Test]
    public void ChannelContext_RoutesToServer()
    {
        _harness.Dispatch(_channel, "JOIN", "#channel");

        A.CallTo(() => _fakeSender.Raw("JOIN #channel")).MustHaveHappened();
    }

    [Test]
    public void PrivateMessageContext_RoutesToServer()
    {
        _harness.Dispatch(_pm, "PRIVMSG", "#chan", ":hello");

        A.CallTo(() => _fakeSender.Raw("PRIVMSG #chan :hello")).MustHaveHappened();
    }

    [Test] // colons, spaces, and a leading + must survive untouched
    public void PreservesColonsAndLeadingPlus()
    {
        _harness.Dispatch(_channel, "PRIVMSG", "#chan", ":+ohai");

        A.CallTo(() => _fakeSender.Raw("PRIVMSG #chan :+ohai")).MustHaveHappened();
    }

    [Test]
    public void EmptyArguments_SendsNothing()
    {
        _harness.Dispatch(_server);

        A.CallTo(() => _fakeSender.Raw(A<string>._)).MustNotHaveHappened();
    }
}
