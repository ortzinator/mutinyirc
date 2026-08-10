using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests;

[TestFixture]
public class ServerPropertiesTests
{
    private Connection _connection;

    [SetUp]
    public void SetupTest() => _connection = new Connection(new ConnectionArgs(), false, false);

    // The 005 reply carries the ISUPPORT tokens. Connection reads them off this line.
    private void Receive005(string tokens) =>
        _connection.Listener.Parse($":irc.fake.com 005 me {tokens} :are supported by this server");

    [Test]
    public void Indexer_MissingKey_ReturnsEmpty()
    {
        Receive005("NETWORK=Libera");

        Assert.AreEqual(string.Empty, _connection.ServerProperties["CHANTYPES"]);
    }

    [Test]
    public void Indexer_FindsATokenTheServerSent()
    {
        Receive005("NETWORK=Libera CHANTYPES=#");

        Assert.AreEqual("Libera", _connection.ServerProperties["NETWORK"]);
    }

    [Test]
    public void Indexer_IgnoresCase()
    {
        // The server always sends the token uppercase, so a case-sensitive lookup means every
        // caller that asks for "Network" gets an empty string and silently concludes the server
        // announced no network.
        Receive005("NETWORK=Libera");

        Assert.AreEqual("Libera", _connection.ServerProperties["Network"]);
    }

    [Test]
    public void RepeatedToken_DoesNotThrowAndKeepsTheLatest()
    {
        // Servers split ISUPPORT over several 005 replies and may repeat a token. Adding a
        // duplicate key must not blow up the parse of an ordinary registration.
        Receive005("NETWORK=Libera");
        Receive005("NETWORK=LiberaChat");

        Assert.AreEqual("LiberaChat", _connection.ServerProperties["NETWORK"]);
    }
}
