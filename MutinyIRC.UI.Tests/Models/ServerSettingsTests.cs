using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

[TestFixture]
public class ServerSettingsTests
{
    // ── Port ─────────────────────────────────────────────────────────────────

    [Test]
    public void Port_ByDefault_IsTheStandardIrcPort()
    {
        // A file that names no port, and a network you add by hand, both land here.
        Assert.That(new ServerSettings().Port, Is.EqualTo(6667));
    }

    [Test]
    public void Constructor_KeepsThePortYouPass()
    {
        var entryPoint = new ServerSettings("irc.libera.chat", 6697, true);

        Assert.Multiple(() =>
        {
            Assert.That(entryPoint.Port, Is.EqualTo(6697));
            Assert.That(entryPoint.Ssl, Is.True);
        });
    }

    // ── FirstPortOf: the legacy "ports" string ───────────────────────────────

    [Test]
    public void FirstPortOf_OnePort_ReturnsIt()
    {
        Assert.That(ServerSettings.FirstPortOf("7000"), Is.EqualTo(7000));
    }

    [Test]
    public void FirstPortOf_AList_TakesTheFirst()
    {
        // Only the first ever mattered: nothing but the random picker read the rest,
        // and nothing read the picker.
        Assert.That(ServerSettings.FirstPortOf("6697,6667,7000"), Is.EqualTo(6697));
    }

    [Test]
    public void FirstPortOf_ARange_TakesItsStart()
    {
        Assert.That(ServerSettings.FirstPortOf("6660-6669"), Is.EqualTo(6660));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("not a port")]
    [TestCase("0")]
    [TestCase("99999")]
    public void FirstPortOf_NothingUsable_FallsBackToTheStandardPort(string? ports)
    {
        // A file we cannot read a port out of must still load, on the port that has
        // always been the default.
        Assert.That(ServerSettings.FirstPortOf(ports), Is.EqualTo(6667));
    }

    // ── Identity ─────────────────────────────────────────────────────────────

    [Test]
    public void Equals_SameUrlDifferentPort_IsEqual()
    {
        // The host identifies an entry point, which is what makes AddServer ignore a
        // host the network already lists.
        var plain = new ServerSettings("irc.libera.chat", 6667, false);
        var secure = new ServerSettings("irc.libera.chat", 6697, true);

        Assert.That(plain.Equals(secure), Is.True);
    }
}
