using System.Collections.Generic;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using NUnit.Framework;

namespace MutinyIRC.Tests;

[TestFixture]
public class AwayCommandTests
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
        // A real Server (not a fake) so its runtime type matches the dispatcher's exact
        // GetType() check for the Server-context overload.
        _server = new Server { Connection = fakeConn };
        _channel = new Channel(_server, "#mutiny");
        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });

        _manager = new PluginManager();
        var type = typeof(Away);
        _manager._commands.Add(type.FullName,
            new CommandInfo(type.Assembly.Location, type.FullName, "away", typeof(ICommand)));
    }

    // Dispatches /away through the real PluginManager so the [RawArguments] opt-out is exercised.
    private void Dispatch(MessageContext context, params string[] args)
        => _manager.ExecuteCommand(new CommandExecutionInfo
        {
            Name = "away",
            Context = context,
            ParameterList = new List<object>(args),
        });

    [Test]
    public void ServerContext_WithMessage_SetsAway()
    {
        Dispatch(_server, "Gone", "fishing");

        A.CallTo(() => _fakeSender.Away("Gone fishing")).MustHaveHappened();
    }

    [Test]
    public void ChannelContext_WithMessage_RoutesToServer()
    {
        Dispatch(_channel, "brb");

        A.CallTo(() => _fakeSender.Away("brb")).MustHaveHappened();
    }

    [Test]
    public void PrivateMessageContext_WithMessage_RoutesToServer()
    {
        Dispatch(_pm, "lunch");

        A.CallTo(() => _fakeSender.Away("lunch")).MustHaveHappened();
    }

    [Test] // bare /away must clear, not call Away("") which the protocol layer rejects
    public void NoArguments_ClearsAway()
    {
        Dispatch(_server);

        A.CallTo(() => _fakeSender.UnAway()).MustHaveHappened();
        A.CallTo(() => _fakeSender.Away(A<string>._)).MustNotHaveHappened();
    }

    [Test] // whitespace-only tail is treated the same as bare /away
    public void WhitespaceOnly_ClearsAway()
    {
        Dispatch(_server, "   ");

        A.CallTo(() => _fakeSender.UnAway()).MustHaveHappened();
        A.CallTo(() => _fakeSender.Away(A<string>._)).MustNotHaveHappened();
    }
}
