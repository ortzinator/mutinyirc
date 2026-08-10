using System.Linq;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

[TestFixture]
public class NetworkSettingsListTests
{
    private static NetworkSettings BuildNetwork(string name, params string[] hosts)
    {
        var net = new NetworkSettings(name);
        foreach (string host in hosts)
            net.AddServer(new ServerSettings(host, "Random", "6667", false));
        return net;
    }

    private static NetworkSettingsList BuildList(params NetworkSettings[] networks)
    {
        var list = new NetworkSettingsList();
        list.ReplaceAll(networks);
        return list;
    }

    // ── Remove: the network you pass is the network that goes ────────────────

    [Test]
    public void Remove_SameNamedNetworks_RemovesTheInstancePassed()
    {
        // The server reported "Libera" for both, which is exactly what identity by name
        // could not survive: the removal must still find the entry points you asked about.
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Libera", "eu.libera.chat");
        var list = BuildList(first, second);

        list.Remove(second);

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0], Is.SameAs(first));
    }

    [Test]
    public void Remove_SameNamedNetworks_KeepsTheOtherEntryPoints()
    {
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Libera", "eu.libera.chat");
        var list = BuildList(first, second);

        list.Remove(second);

        Assert.That(list[0].Servers.Select(s => s.Url),
            Is.EqualTo(new[] { "irc.libera.chat" }));
    }

    [Test]
    public void Remove_NetworkNotInList_ReturnsFalse()
    {
        var list = BuildList(BuildNetwork("Libera", "irc.libera.chat"));

        bool removed = list.Remove(BuildNetwork("Libera", "irc.libera.chat"));

        Assert.That(removed, Is.False);
        Assert.That(list.Count, Is.EqualTo(1));
    }

    // ── GetOrAddNetwork ──────────────────────────────────────────────────────

    [Test]
    public void GetOrAddNetwork_EmptyList_AddsTheNetwork()
    {
        var list = new NetworkSettingsList();

        NetworkSettings net = list.GetOrAddNetwork("Libera");

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0], Is.SameAs(net));
    }

    [Test]
    public void GetOrAddNetwork_TwiceWithDifferentNames_GivesTwoNetworks()
    {
        var list = new NetworkSettingsList();

        list.GetOrAddNetwork("Libera");
        list.GetOrAddNetwork("OFTC");

        Assert.That(list.Select(n => n.Name), Is.EqualTo(new[] { "Libera", "OFTC" }));
    }

    [Test]
    public void GetOrAddNetwork_TwiceWithNoName_DoesNotGiveTheSameNetworkTwice()
    {
        // Two empty networks are not one network. Set equality on an empty entry-point
        // set would make them one and silently drop the second host.
        var list = new NetworkSettingsList();

        NetworkSettings first = list.GetOrAddNetwork("irc.example.net");
        NetworkSettings second = list.GetOrAddNetwork("irc.example.org");

        Assert.That(second, Is.Not.SameAs(first));
        Assert.That(list.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetOrAddNetwork_KnownName_GivesTheNetworkThatHasIt()
    {
        var libera = BuildNetwork("Libera", "irc.libera.chat");
        var list = BuildList(libera);

        NetworkSettings net = list.GetOrAddNetwork("Libera");

        Assert.That(net, Is.SameAs(libera));
        Assert.That(list.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetOrAddNetwork_KnownName_TakesTheNewEntryPoint()
    {
        // A host that reports a known name joins that network instead of standing alone.
        var libera = BuildNetwork("Libera", "irc.libera.chat");
        var list = BuildList(libera);

        list.GetOrAddNetwork("Libera")
            .AddServer(new ServerSettings("eu.libera.chat", "Random", "6667", false));

        Assert.That(libera.Servers.Select(s => s.Url),
            Is.EqualTo(new[] { "irc.libera.chat", "eu.libera.chat" }));
    }

    [Test]
    public void GetOrAddNetwork_NameInAnotherCase_GivesTheNetworkThatHasIt()
    {
        var libera = BuildNetwork("Libera", "irc.libera.chat");
        var list = BuildList(libera);

        Assert.That(list.GetOrAddNetwork("LIBERA"), Is.SameAs(libera));
    }

    // ── GetNetwork ───────────────────────────────────────────────────────────

    [Test]
    public void GetNetwork_UnknownName_ReturnsNull()
    {
        var list = BuildList(BuildNetwork("Libera", "irc.libera.chat"));

        Assert.That(list.GetNetwork("OFTC"), Is.Null);
    }

    [Test]
    public void GetNetwork_DoesNotAdd()
    {
        var list = BuildList(BuildNetwork("Libera", "irc.libera.chat"));

        list.GetNetwork("OFTC");

        Assert.That(list.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetNetwork_SameNamedNetworks_GivesTheFirst()
    {
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Libera", "eu.libera.chat");
        var list = BuildList(first, second);

        Assert.That(list.GetNetwork("Libera"), Is.SameAs(first));
    }

    // ── ReplaceAll: one hostname belongs to one network ──────────────────────

    [Test]
    public void ReplaceAll_SwapsTheWholeSet()
    {
        var list = BuildList(BuildNetwork("Libera", "irc.libera.chat"));

        list.ReplaceAll(new[] { BuildNetwork("OFTC", "irc.oftc.net") });

        Assert.That(list.Select(n => n.Name), Is.EqualTo(new[] { "OFTC" }));
    }

    [Test]
    public void ReplaceAll_HostClaimedTwice_KeepsItOnTheFirstNetwork()
    {
        // A URL lookup stops at the first network that lists the host, so the second claim was
        // already unreachable. Saving it would leave settings that disagree with lookup.
        var first = BuildNetwork("Libera", "irc.libera.chat");
        var second = BuildNetwork("Mirror", "irc.libera.chat", "eu.libera.chat");
        var list = new NetworkSettingsList();

        list.ReplaceAll(new[] { first, second });

        Assert.That(first.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
        Assert.That(second.Servers.Select(s => s.Url), Is.EqualTo(new[] { "eu.libera.chat" }),
            "The duplicate entry point must be dropped from the later network, not the network itself");
    }

    [Test]
    public void ReplaceAll_HostClaimedTwice_KeepsBothNetworks()
    {
        var list = new NetworkSettingsList();

        list.ReplaceAll(new[]
        {
            BuildNetwork("Libera", "irc.libera.chat"),
            BuildNetwork("Mirror", "irc.libera.chat")
        });

        Assert.That(list.Count, Is.EqualTo(2),
            "Dropping a duplicate host must not delete the network that held it, along with its channels");
    }

    [Test]
    public void ReplaceAll_HostInAnotherCase_CountsAsTheSameHost()
    {
        var list = new NetworkSettingsList();

        list.ReplaceAll(new[]
        {
            BuildNetwork("Libera", "irc.libera.chat"),
            BuildNetwork("Mirror", "IRC.Libera.Chat")
        });

        Assert.That(list[1].Servers, Is.Empty);
    }
}
