using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests;

/// <summary>
/// The CTCP listener sorts each CTCP message into a request, a reply, a ping request or a ping
/// reply, and gives the correct data to each event. The Connection is never connected: a send
/// fails without an exception, but <see cref="Connection.RawMessageSent"/> still shows the text.
/// </summary>
[TestFixture]
public class CtcpListenerTests
{
    private const string From = "Buster!~buster@buster.host";

    private Connection _connection;
    private CtcpListener _ctcp;

    [SetUp]
    public void SetUp()
    {
        _connection = new Connection(new ConnectionArgs("test", "irc.fake.com", false), true, false);
        _ctcp = _connection.CtcpListener;
    }

    private static string Ctcp(string verb, string body) =>
        $":{From} {verb} OrtzIRC :\u0001{body}\u0001";

    [Test]
    public void Parse_Request_FiresOnCtcpRequest()
    {
        CtcpRequestEventArgs givenArgs = null;
        _ctcp.OnCtcpRequest += (_, e) => givenArgs = e;

        _ctcp.Parse(Ctcp("PRIVMSG", "VERSION"));

        Assert.AreEqual(CtcpUtil.Version, givenArgs.Command);
        Assert.AreEqual("Buster", givenArgs.User.Nick);
    }

    [Test]
    public void Parse_Reply_FiresOnCtcpReply()
    {
        CtcpReplyEventArgs givenArgs = null;
        _ctcp.OnCtcpReply += (_, e) => givenArgs = e;

        _ctcp.Parse(Ctcp("NOTICE", "VERSION BusterClient 2.0"));

        Assert.AreEqual(CtcpUtil.Version, givenArgs.Command);
        Assert.AreEqual("Buster", givenArgs.User.Nick);
        Assert.AreEqual("BusterClient 2.0", givenArgs.Reply);
    }

    [Test]
    public void Parse_PingFromAnotherUser_FiresOnCtcpPingRequest()
    {
        CtcpPingEventArgs givenArgs = null;
        _ctcp.OnCtcpPingRequest += (_, e) => givenArgs = e;

        _ctcp.Parse(Ctcp("PRIVMSG", "PING 123456"));

        Assert.AreEqual("Buster", givenArgs.User.Nick);
        Assert.AreEqual("123456", givenArgs.Timestamp);
    }

    [Test]
    public void Parse_PingWithOurTimestamp_FiresOnCtcpPingReply()
    {
        CtcpPingEventArgs givenArgs = null;
        bool requestFired = false;
        _ctcp.OnCtcpPingReply += (_, e) => givenArgs = e;
        _ctcp.OnCtcpPingRequest += (_, _) => requestFired = true;
        _connection.CtcpSender.CtcpPingRequest("Buster", "123456");

        _ctcp.Parse(Ctcp("NOTICE", "PING 123456"));

        Assert.AreEqual("Buster", givenArgs.User.Nick);
        Assert.AreEqual("123456", givenArgs.Timestamp);
        Assert.IsFalse(requestFired, "Our own timestamp is a reply, not a request");
    }

    [Test]
    public void Responder_PingRequest_SendsThePingReply()
    {
        string sent = null;
        _connection.RawMessageSent += (_, e) => sent = e.Data;
        _connection.CtcpResponder = new CtcpResponder(_connection);

        _ctcp.Parse(Ctcp("PRIVMSG", "PING 123456"));

        Assert.AreEqual("NOTICE Buster :\u0001PING 123456\u0001", sent);
    }
}
