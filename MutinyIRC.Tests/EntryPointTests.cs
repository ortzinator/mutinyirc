using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.Tests;

/// <summary>
/// Covers how <see cref="EntryPoint.TryParse"/> reads what the user types into the Add Server
/// box or after <c>/server</c>.
/// </summary>
[TestFixture]
public class EntryPointTests
{
    [Test]
    public void TryParse_BareHost_TakesTheTlsPort()
    {
        Assert.That(EntryPoint.TryParse("irc.libera.chat", out EntryPoint entry), Is.True);
        Assert.That(entry, Is.EqualTo(new EntryPoint("irc.libera.chat", 6697, true)));
    }

    [Test]
    public void TryParse_HostAndPort_TakesThePort()
    {
        EntryPoint.TryParse("irc.oftc.net:7000", out EntryPoint entry);

        Assert.That(entry, Is.EqualTo(new EntryPoint("irc.oftc.net", 7000, true)));
    }

    [TestCase(6667)]
    [TestCase(6665)]
    public void TryParse_APlaintextPort_IsNotTls(int port)
    {
        // The port is the only thing we know about a host we have never dialled, so the
        // two ports that historically mean plaintext are what decide.
        EntryPoint.TryParse($"irc.libera.chat:{port}", out EntryPoint entry);

        Assert.That(entry.Ssl, Is.False);
    }

    [Test]
    public void TryParse_AnIpAddress_Parses()
    {
        EntryPoint.TryParse("127.0.0.1:6698", out EntryPoint entry);

        Assert.That(entry.Host, Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public void TryParse_TrimsSurroundingSpace()
    {
        EntryPoint.TryParse("  irc.libera.chat  ", out EntryPoint entry);

        Assert.That(entry.Host, Is.EqualTo("irc.libera.chat"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("irc libera chat")]
    [TestCase("irc.libera.chat:")]
    [TestCase("irc.libera.chat:port")]
    [TestCase("irc.libera.chat:6")]
    [TestCase("irc.libera.chat:00")]
    [TestCase("irc.libera.chat:99999")]
    [TestCase("irc.libera.chat:999999")]
    [TestCase("irc.libera.chat:+6697")]
    [TestCase("irc.libera.chat:٦٦٩٧")]
    [TestCase("irc.libera.chat:6697:6667")]
    [TestCase("irc://libera.chat")]
    public void TryParse_Malformed_Fails(string text)
    {
        Assert.That(EntryPoint.TryParse(text, out _), Is.False);
    }
}
