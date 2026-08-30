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
            net.AddServer(new ServerSettings(host, 6667, false));
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

        net.AddServer(new ServerSettings("irc.libera.chat", 7000, true));

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

    // ── MoveServer: the failover order ───────────────────────────────────────

    [Test]
    public void MoveServer_Later_SwapsWithTheOneBelow()
    {
        var net = BuildNetwork("Libera", "a", "b", "c");

        Assert.That(net.MoveServer(net.Servers[0], 1), Is.True);
        Assert.That(net.Servers.Select(s => s.Url), Is.EqualTo(new[] { "b", "a", "c" }));
    }

    [Test]
    public void MoveServer_Earlier_SwapsWithTheOneAbove()
    {
        var net = BuildNetwork("Libera", "a", "b", "c");

        Assert.That(net.MoveServer(net.Servers[2], -1), Is.True);
        Assert.That(net.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "c", "b" }));
    }

    [Test]
    public void MoveServer_OffTheTop_ChangesNothing()
    {
        var net = BuildNetwork("Libera", "a", "b");

        Assert.That(net.MoveServer(net.Servers[0], -1), Is.False);
        Assert.That(net.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void MoveServer_OffTheBottom_ChangesNothing()
    {
        var net = BuildNetwork("Libera", "a", "b");

        Assert.That(net.MoveServer(net.Servers[1], 1), Is.False);
        Assert.That(net.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void MoveServer_AHostThisNetworkDoesNotList_ChangesNothing()
    {
        var net = BuildNetwork("Libera", "a");

        Assert.That(net.MoveServer(new ServerSettings("elsewhere", 6667, false), 1), Is.False);
    }

    // ── ToConnectionCandidates ───────────────────────────────────────────────

    [Test]
    public void ToConnectionCandidates_KeepsTheSavedPort()
    {
        // ConnectionArgs' constructor hardcodes 6667, so a port set only there is
        // dropped at dial time. This is the bug the migration fixes.
        var net = new NetworkSettings("Libera");
        net.AddServer(new ServerSettings("irc.libera.chat", 6697, true));

        var candidates = net.ToConnectionCandidates();

        Assert.Multiple(() =>
        {
            Assert.That(candidates[0].Port, Is.EqualTo(6697));
            Assert.That(candidates[0].Ssl, Is.True);
            Assert.That(candidates[0].Hostname, Is.EqualTo("irc.libera.chat"));
        });
    }

    [Test]
    public void ToConnectionCandidates_KeepsTheFailoverOrder()
    {
        var net = BuildNetwork("Libera", "first", "second", "third");

        Assert.That(net.ToConnectionCandidates().Select(c => c.Hostname),
            Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public void ToConnectionCandidates_UsesTheNetworkNick()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");
        net.Nick = "sparkplug";

        Assert.That(net.ToConnectionCandidates()[0].Nick, Is.EqualTo("sparkplug"));
    }

    [Test]
    public void ToConnectionCandidates_NoNetworkNick_FallsBackToTheAppNick()
    {
        var net = BuildNetwork("Libera", "irc.libera.chat");

        Assert.That(net.ToConnectionCandidates()[0].Nick,
            Is.EqualTo(AppSettings.Instance.FirstNick));
    }

    [Test]
    public void ToConnectionCandidates_NoEntryPoints_IsEmpty()
    {
        Assert.That(new NetworkSettings("Libera").ToConnectionCandidates(), Is.Empty);
    }

    // ── Channels ─────────────────────────────────────────────────────────────

    [Test]
    public void RemoveChannel_DropsIt()
    {
        // Membership of the list is the autojoin flag, so parting a channel takes it
        // off the list rather than clearing a flag on it.
        var net = new NetworkSettings("Libera");
        net.AddChannel(new Common.ChannelSettings("#mutinyirc", true));

        Assert.That(net.RemoveChannel(net.Channels[0]), Is.True);
        Assert.That(net.Channels, Is.Empty);
    }
}
