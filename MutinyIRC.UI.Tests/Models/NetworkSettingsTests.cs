using System.Collections.Generic;
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

    [Test]
    public void Equals_SameInstance_IsEqual()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        Assert.That(net.Equals(net), Is.True);
    }

    [Test]
    public void Contains_HashedLookupAndListScanAgree()
    {
        // A hashed lookup goes through GetHashCode and a list scan goes through Equals.
        // Name equality with no matching GetHashCode made the two disagree about whether
        // a same-named network was already there.
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Libera", "eu.libera.chat");

        var set = new HashSet<NetworkSettings> { first };
        var list = new List<NetworkSettings> { first };

        Assert.That(set.Contains(second), Is.EqualTo(list.Contains(second)));
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
}
