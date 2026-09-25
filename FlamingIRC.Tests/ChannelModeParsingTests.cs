using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests;

/// <summary>
/// The mode parser gives each parameter to the correct mode, from the CHANMODES and PREFIX
/// tokens that the server sends in its '005' reply.
/// </summary>
[TestFixture]
public class ChannelModeParsingTests
{
    private const string Libera = "CHANMODES=eIbq,k,flj,CFLMPQScgimnprstuz PREFIX=(ov)@+";

    private Connection _connection;
    private ChannelModeInfo[] _modes;

    [SetUp]
    public void SetupTest()
    {
        _connection = new Connection(new ConnectionArgs(), false, false);
        _modes = null;
        _connection.Listener.OnChannelModeChange += (who, channel, modes, raw) => _modes = modes;
    }

    private void Receive005(string tokens) =>
        _connection.Listener.Parse($":irc.fake.com 005 me {tokens} :are supported by this server");

    private void ReceiveMode(string modeAndParameters) =>
        _connection.Listener.Parse($":Op!~op@op.example MODE #ortzirc {modeAndParameters}");

    [Test]
    public void ListMode_FromChanModes_TakesItsMask()
    {
        // On Libera, +q is the quiet list. If the parser does not give it the mask, 'o'
        // gets the mask and the nick is lost.
        Receive005(Libera);

        ReceiveMode("+qo *!*@spammer.example SomeNick");

        Assert.AreEqual(2, _modes.Length);
        Assert.AreEqual(ChannelMode.Quiet, _modes[0].Mode);
        Assert.AreEqual("*!*@spammer.example", _modes[0].Parameter);
        Assert.AreEqual('\0', _modes[0].PrefixSymbol, "The quiet list gives no status");
        Assert.AreEqual(ChannelMode.ChannelOperator, _modes[1].Mode);
        Assert.AreEqual("SomeNick", _modes[1].Parameter);
        Assert.AreEqual('@', _modes[1].PrefixSymbol);
    }

    [Test]
    public void SetOnlyMode_TakesAParameterWhenSetButNotWhenUnset()
    {
        // Libera puts 'j' and 'l' in group C: they take a parameter only when they are set.
        Receive005(Libera);

        ReceiveMode("+j-lo 3:5 SomeNick");

        Assert.AreEqual(3, _modes.Length);
        Assert.AreEqual("3:5", _modes[0].Parameter);
        Assert.AreEqual(ChannelMode.UserLimit, _modes[1].Mode);
        Assert.AreEqual("", _modes[1].Parameter);
        Assert.AreEqual("SomeNick", _modes[2].Parameter);
    }

    [Test]
    public void PrefixMode_FromPrefixToken_TakesANickAndItsSymbol()
    {
        // On servers with an owner prefix, 'q' is a status mode and takes a nick. It is in
        // PREFIX, not in CHANMODES, and PREFIX gives it the '~' symbol.
        Receive005("CHANMODES=beI,k,l,imnpst PREFIX=(qaohv)~&@%+");

        ReceiveMode("+qv Owner Voiced");

        Assert.AreEqual(2, _modes.Length);
        Assert.AreEqual("Owner", _modes[0].Parameter);
        Assert.AreEqual('~', _modes[0].PrefixSymbol);
        Assert.AreEqual("Voiced", _modes[1].Parameter);
        Assert.AreEqual('+', _modes[1].PrefixSymbol);
    }

    [Test]
    public void MalformedPrefix_UsesTheDefault()
    {
        // The symbol count does not match the mode count, so the parser cannot pair them.
        Receive005("CHANMODES=beI,k,l,imnpst PREFIX=(qov)@+");

        ReceiveMode("+qo SomeNick");

        Assert.AreEqual("", _modes[0].Parameter);
        Assert.AreEqual("SomeNick", _modes[1].Parameter);
        Assert.AreEqual('@', _modes[1].PrefixSymbol);
    }

    [Test]
    public void ModeInNoGroup_TakesNoParameter()
    {
        // 'q' is not in any group on this server, so the nick goes to 'o'.
        Receive005("CHANMODES=beI,k,l,imnpst PREFIX=(ov)@+");

        ReceiveMode("+qo SomeNick");

        Assert.AreEqual("", _modes[0].Parameter);
        Assert.AreEqual("SomeNick", _modes[1].Parameter);
    }

    [Test]
    public void BeforeAny005_UsesTheRfc2812Modes()
    {
        ReceiveMode("+kloO secret 10 SomeNick Creator");

        Assert.AreEqual(4, _modes.Length);
        Assert.AreEqual("secret", _modes[0].Parameter);
        Assert.AreEqual("10", _modes[1].Parameter);
        Assert.AreEqual("SomeNick", _modes[2].Parameter);
        Assert.AreEqual('@', _modes[2].PrefixSymbol);
        Assert.AreEqual("Creator", _modes[3].Parameter);
        Assert.AreEqual('\0', _modes[3].PrefixSymbol, "The channel creator mode gives no status");
    }

    [Test]
    public void StandaloneListener_RecordsThe005()
    {
        // A caller can use a Listener without a Connection. It must still read CHANMODES.
        var listener = new Listener();
        listener.OnChannelModeChange += (who, channel, modes, raw) => _modes = modes;

        listener.Parse($":irc.fake.com 005 me {Libera} :are supported by this server");
        listener.Parse(":Op!~op@op.example MODE #ortzirc +qo *!*@spammer.example SomeNick");

        Assert.AreEqual("*!*@spammer.example", _modes[0].Parameter);
        Assert.AreEqual("SomeNick", _modes[1].Parameter);
    }

    [Test]
    public void ChannelModeReply_UsesTheServerModes()
    {
        // The '324' reply to a MODE query goes through the same parser as a MODE change.
        // Unreal puts 'f' in group B.
        Receive005("CHANMODES=beIqa,kfL,lH,psmntirRcOAQKVCuzNSMTGZ PREFIX=(qaohv)~&@%+");
        ChannelModeInfo[] reply = null;
        _connection.Listener.OnChannelModeRequest += (channel, modes) => reply = modes;

        _connection.Listener.Parse(":irc.fake.com 324 me #ortzirc +fnt [5j]:10");

        Assert.AreEqual(3, reply.Length);
        Assert.AreEqual("[5j]:10", reply[0].Parameter);
        Assert.AreEqual("", reply[1].Parameter);
    }
}
