using System.Linq;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

[TestFixture]
public class NetworkSettingsTests
{
    private static NetworkSettings BuildNetwork(string name, params string[] hosts)
    {
        var net = new NetworkSettings(name);
        foreach (string host in hosts)
            net.AddServer(new ServerSettings(host, "Random", "6667", false));
        return net;
    }

    // ── Identity ─────────────────────────────────────────────────────────────

    [Test]
    public void Equals_SameName_IsNotEqual()
    {
        // The server owns the name and can change it on any connect, so the name must
        // not make two networks one.
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Libera", "eu.libera.chat");

        Assert.That(first.Equals(second), Is.False);
    }

    // ── AddServer ────────────────────────────────────────────────────────────

    [Test]
    public void AddServer_SameUrlTwice_KeepsOne()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        net.AddServer(new ServerSettings("irc.libera.chat", "Random", "7000", true));

        Assert.That(net.Servers.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddServer_SetsBackReferenceToNetwork()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        Assert.That(net.Servers[0].Network, Is.SameAs(net));
    }

    [Test]
    public void GetServer_MatchesUrlIgnoringCase()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        Assert.That(net.GetServer("IRC.Libera.Chat"), Is.SameAs(net.Servers[0]));
    }

    [Test]
    public void RemoveServer_DropsTheEntryPointYouPass()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat", "eu.libera.chat");

        net.RemoveServer(net.Servers[1]);

        Assert.That(net.Servers.Select(s => s.Url),
            Is.EqualTo(new[] { "irc.libera.chat" }));
    }

    // ── GetRandomServer ──────────────────────────────────────────────────────

    [Test]
    public void GetRandomServer_OverManyDraws_ReachesEveryEntryPoint()
    {
        // The entry points of a network are interchangeable, so each one must be
        // reachable. An exclusive upper bound made the last one unreachable.
        // 300 draws over 3 entry points: a miss is about 1 in 10^52.
        var net = BuildNetwork("Libera", "irc.libera.chat", "eu.libera.chat", "us.libera.chat");

        var seen = Enumerable.Range(0, 300)
            .Select(_ => net.GetRandomServer().Url)
            .Distinct();

        Assert.That(seen, Is.EquivalentTo(new[]
        {
            "irc.libera.chat", "eu.libera.chat", "us.libera.chat"
        }));
    }

    [Test]
    public void GetRandomServer_OneEntryPoint_ReturnsIt()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        Assert.That(net.GetRandomServer(), Is.SameAs(net.Servers[0]));
    }

    [Test]
    public void GetRandomServer_NoEntryPoints_ThrowsInvalidOperation()
    {
        var net = new NetworkSettings("Libera");

        Assert.That(() => net.GetRandomServer(), Throws.InvalidOperationException);
    }
}
