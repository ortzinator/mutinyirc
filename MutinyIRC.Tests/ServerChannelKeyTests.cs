using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace MutinyIRC.Tests;

/// <summary>
/// Pins how a channel's key reaches <see cref="Channel.Key"/> from the wire: the MODE request
/// the Server sends on its own join, the reply to it, and later MODE changes. The fake sender
/// records the request without opening a socket; the Listener is real, so the tests drive the
/// same parse path the app does.
/// </summary>
[TestFixture]
public class ServerChannelKeyTests
{
    private ISender _sender;
    private Server _server;

    [SetUp]
    public void Setup()
    {
        _sender = A.Fake<ISender>();
        var conn = A.Fake<IConnection>();
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.Sender).Returns(_sender);
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("test", "irc.fake.com", false));
        _server = new Server(conn);
    }

    [TearDown]
    public void Teardown()
    {
        _server = null;
    }

    private void SelfJoin(string channel) =>
        _server.Connection.Listener.Parse($":test!u@h JOIN :{channel}");

    [Test]
    public void SelfJoin_RequestsTheChannelModes()
    {
        SelfJoin("#mutiny");

        A.CallTo(() => _sender.RequestChannelModes("#mutiny")).MustHaveHappened();
    }

    [Test]
    public void SelfJoin_ByAnotherUser_DoesNotRequestTheChannelModes()
    {
        _server.Channels.Add("#mutiny", new Channel(_server, "#mutiny"));

        _server.Connection.Listener.Parse(":bob!u@h JOIN :#mutiny");

        A.CallTo(() => _sender.RequestChannelModes(A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public void ModeReply_WithAKey_StoresItOnTheChannel()
    {
        SelfJoin("#mutiny");

        _server.Connection.Listener.Parse(":server.name 324 test #mutiny +ntk hunter2");

        Assert.AreEqual("hunter2", _server.Channels["#mutiny"].Key);
    }

    [Test]
    public void ModeReply_WithoutAKey_LeavesTheChannelUnkeyed()
    {
        SelfJoin("#mutiny");
        _server.Connection.Listener.Parse(":bob!u@h MODE #mutiny +k hunter2");

        _server.Connection.Listener.Parse(":server.name 324 test #mutiny +nt");

        Assert.IsNull(_server.Channels["#mutiny"].Key,
            "A mode reply reports every mode the channel has, so one with no key means unkeyed");
    }

    [Test]
    public void ModeReply_ForAnUnknownChannel_IsIgnored()
    {
        Assert.DoesNotThrow(() =>
            _server.Connection.Listener.Parse(":server.name 324 test #elsewhere +ntk hunter2"));
    }

    [Test]
    public void ModeChange_SettingAKey_StoresItOnTheChannel()
    {
        SelfJoin("#mutiny");

        _server.Connection.Listener.Parse(":bob!u@h MODE #mutiny +k hunter2");

        Assert.AreEqual("hunter2", _server.Channels["#mutiny"].Key);
    }

    [Test]
    public void ModeChange_ClearingAKey_ClearsItOnTheChannel()
    {
        SelfJoin("#mutiny");
        _server.Connection.Listener.Parse(":bob!u@h MODE #mutiny +k hunter2");

        _server.Connection.Listener.Parse(":bob!u@h MODE #mutiny -k hunter2");

        Assert.IsNull(_server.Channels["#mutiny"].Key);
    }
}
