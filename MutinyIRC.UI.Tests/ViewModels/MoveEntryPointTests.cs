using System.IO;
using System.Linq;
using MutinyIRC.Common;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the move of an entry point to another network in the Networks dialog: which networks
/// the Move to menu lists, where the entry point goes, what the network it leaves keeps, and the
/// status line. The manager points at a file of this fixture's own.
/// </summary>
[TestFixture]
public class MoveEntryPointTests
{
    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _settingsFile = null!;

    [SetUp]
    public void SetUp() =>
        _settingsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void TearDown() => File.Delete(_settingsFile);

    // Libera (two entry points, the first pinned), OFTC (one), and an empty minted network.
    private SettingsViewModel Dialog(out IrcSettingsManager manager)
    {
        var libera = new NetworkSettings("Libera")
        {
            Nick = "sparkplug",
            AlternateNick = "sparkplug_",
            AutoConnect = true,
            VerifyCertificates = false,
        };
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true) { PinnedFingerprint = Fingerprint });
        libera.AddServer(new ServerSettings("eu.libera.chat", 6667, false));
        libera.AddChannel(new ChannelSettings("#mutiny", true, string.Empty, string.Empty));

        var oftc = new NetworkSettings("OFTC");
        oftc.AddServer(new ServerSettings("irc.oftc.net", 6697, true));

        var minted = new NetworkSettings("Minted") { MintedByPin = true };

        manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(new[] { libera, oftc, minted });
        return new SettingsViewModel(manager);
    }

    private static NetworkSettingsViewModel Net(SettingsViewModel vm, string name) =>
        vm.Networks.Single(n => n.Name == name);

    [Test]
    public void MoveTargets_ListsEveryOtherNetworkInDialogOrder()
    {
        var vm = Dialog(out _);

        Assert.That(Net(vm, "OFTC").MoveTargets.Select(n => n.Name),
            Is.EqualTo(new[] { "Libera", "Minted" }));
    }

    [Test]
    public void MoveTargets_FollowAnAddedOrRenamedNetwork()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        int notified = 0;
        libera.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NetworkSettingsViewModel.MoveTargets))
                notified++;
        };

        vm.AddNetworkCommand.Execute(null);
        Net(vm, "OFTC").Name = "OFTC2";

        Assert.Multiple(() =>
        {
            Assert.That(libera.MoveTargets.Select(n => n.Name),
                Is.EqualTo(new[] { "OFTC2", "Minted", "New Network" }));
            Assert.That(notified, Is.GreaterThanOrEqualTo(2));
        });
    }

    [Test]
    public void Move_PutsTheEntryPointLastInTheTarget_WithItsPin()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        NetworkSettingsViewModel oftc = Net(vm, "OFTC");
        ServerSettingsViewModel row = libera.Servers[0];

        libera.MoveServerTo(row, oftc);

        Assert.Multiple(() =>
        {
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "eu.libera.chat" }));
            Assert.That(oftc.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.oftc.net", "irc.libera.chat" }));
            Assert.That(row.PinnedFingerprint, Is.EqualTo(Fingerprint));
            Assert.That(row.Order, Is.EqualTo(2));
            Assert.That(row.IsFirst, Is.False);
            Assert.That(libera.Servers[0].Order, Is.EqualTo(1));
            Assert.That(libera.Servers[0].IsFirst, Is.True);
        });
    }

    [Test]
    public void Move_ThenSave_KeepsTheEntryPointLastInTheTarget_WithItsPin()
    {
        var vm = Dialog(out IrcSettingsManager manager);
        Net(vm, "Libera").MoveServerTo(Net(vm, "Libera").Servers[0], Net(vm, "OFTC"));

        vm.Save();

        NetworkSettings oftc = manager.Networks.Single(n => n.Name == "OFTC");
        Assert.Multiple(() =>
        {
            Assert.That(oftc.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.oftc.net", "irc.libera.chat" }));
            Assert.That(oftc.Servers[1].Port, Is.EqualTo(6697));
            Assert.That(oftc.Servers[1].Ssl, Is.True);
            Assert.That(oftc.Servers[1].PinnedFingerprint, Is.EqualTo(Fingerprint));
            Assert.That(manager.Networks.Single(n => n.Name == "Libera").Servers.Select(s => s.Url),
                Is.EqualTo(new[] { "eu.libera.chat" }));
        });
    }

    [Test]
    public void MovingTheLastEntryPoint_LeavesTheSourceNetworkWithItsSettings()
    {
        var vm = Dialog(out IrcSettingsManager manager);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        NetworkSettingsViewModel oftc = Net(vm, "OFTC");

        libera.MoveServerTo(libera.Servers[0], oftc);
        libera.MoveServerTo(libera.Servers[0], oftc);
        vm.Save();

        NetworkSettings saved = manager.Networks.Single(n => n.Name == "Libera");
        Assert.Multiple(() =>
        {
            Assert.That(vm.Networks, Does.Contain(libera));
            Assert.That(saved.Servers, Is.Empty);
            Assert.That(saved.Nick, Is.EqualTo("sparkplug"));
            Assert.That(saved.AlternateNick, Is.EqualTo("sparkplug_"));
            Assert.That(saved.AutoConnect, Is.True);
            Assert.That(saved.VerifyCertificates, Is.False);
            Assert.That(saved.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#mutiny" }));
        });
    }

    [Test]
    public void Move_DoesNotChangeMintedByPinOnEitherNetwork()
    {
        var vm = Dialog(out IrcSettingsManager manager);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        NetworkSettingsViewModel minted = Net(vm, "Minted");

        libera.MoveServerTo(libera.Servers[1], minted);
        minted.MoveServerTo(minted.Servers[0], Net(vm, "OFTC"));
        libera.MoveServerTo(libera.Servers[0], minted);
        vm.Save();

        Assert.Multiple(() =>
        {
            Assert.That(manager.Networks.Single(n => n.Name == "Minted").MintedByPin, Is.True);
            Assert.That(manager.Networks.Single(n => n.Name == "Libera").MintedByPin, Is.False);
            Assert.That(manager.Networks.Single(n => n.Name == "OFTC").MintedByPin, Is.False);
        });
    }

    [Test]
    public void Move_ToTheSameNetworkOrARowOfAnotherNetwork_DoesNothing()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        NetworkSettingsViewModel oftc = Net(vm, "OFTC");

        libera.MoveServerTo(libera.Servers[0], libera);
        libera.MoveServerTo(oftc.Servers[0], Net(vm, "Minted"));

        Assert.Multiple(() =>
        {
            Assert.That(libera.Servers, Has.Count.EqualTo(2));
            Assert.That(oftc.Servers, Has.Count.EqualTo(1));
            Assert.That(libera.HasMoveStatus, Is.False);
        });
    }

    [Test]
    public void Move_ShowsTheStatusLineInTheNetworkItLeaves()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        NetworkSettingsViewModel oftc = Net(vm, "OFTC");

        libera.MoveServerTo(libera.Servers[0], oftc);

        Assert.Multiple(() =>
        {
            Assert.That(libera.MoveStatus, Is.EqualTo("Moved irc.libera.chat to OFTC."));
            Assert.That(libera.HasMoveStatus, Is.True);
            Assert.That(oftc.HasMoveStatus, Is.False);
        });
    }

    // Each case is a next action in the network that the entry point left.
    private static readonly object[] NextActions =
    {
        new object[] { "add a server", (System.Action<NetworkSettingsViewModel>)(n =>
        {
            n.NewServerText = "irc.example.net";
            n.AddServerCommand.Execute(null);
        }) },
        new object[] { "type in the Add box", (System.Action<NetworkSettingsViewModel>)(n => n.NewServerText = "x") },
        new object[] { "remove a server", (System.Action<NetworkSettingsViewModel>)(n => n.RemoveServerCommand.Execute(n.Servers[0])) },
        new object[] { "reorder", (System.Action<NetworkSettingsViewModel>)(n =>
        {
            n.NewServerText = "irc.example.net";
            n.AddServerCommand.Execute(null);
            n.MoveServerUpCommand.Execute(n.Servers[1]);
        }) },
        new object[] { "open an editor", (System.Action<NetworkSettingsViewModel>)(n => n.EditServerCommand.Execute(n.Servers[0])) },
        new object[] { "forget a pin", (System.Action<NetworkSettingsViewModel>)(n => n.Servers[0].ForgetPinCommand.Execute(null)) },
        new object[] { "rename", (System.Action<NetworkSettingsViewModel>)(n => n.BeginEdit()) },
        new object[] { "change the nick", (System.Action<NetworkSettingsViewModel>)(n => n.Nick = "other") },
        new object[] { "change the alternate nick", (System.Action<NetworkSettingsViewModel>)(n => n.AlternateNick = "other") },
        new object[] { "flip autoconnect", (System.Action<NetworkSettingsViewModel>)(n => n.AutoConnect = !n.AutoConnect) },
        new object[] { "flip verify", (System.Action<NetworkSettingsViewModel>)(n => n.VerifyCertificates = !n.VerifyCertificates) },
        new object[] { "start a channel", (System.Action<NetworkSettingsViewModel>)(n => n.BeginAddChannelCommand.Execute(null)) },
        new object[] { "remove a channel", (System.Action<NetworkSettingsViewModel>)(n => n.RemoveChannelCommand.Execute(n.Channels[0])) },
        new object[] { "move another server", (System.Action<NetworkSettingsViewModel>)(n => n.MoveServerTo(n.Servers[0], n.MoveTargets[1])) },
    };

    [TestCaseSource(nameof(NextActions))]
    public void TheNextActionInTheNetwork_ClearsTheStatusLine(string action, System.Action<NetworkSettingsViewModel> act)
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        // The first entry point has the pin, so move the second one.
        libera.MoveServerTo(libera.Servers[1], Net(vm, "OFTC"));
        Assert.That(libera.HasMoveStatus, Is.True);

        act(libera);

        if (action == "move another server")
            Assert.That(libera.MoveStatus, Is.EqualTo("Moved irc.libera.chat to Minted."), action);
        else
            Assert.That(libera.HasMoveStatus, Is.False, action);
    }

    [Test]
    public void AnActionInAnotherNetwork_KeepsTheStatusLine()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        libera.MoveServerTo(libera.Servers[1], Net(vm, "OFTC"));

        Net(vm, "Minted").Nick = "someone";
        vm.SelectedNetwork = Net(vm, "Minted");

        Assert.That(libera.MoveStatus, Is.EqualTo("Moved eu.libera.chat to OFTC."));
    }

    [Test]
    public void Move_WhileTheRowIsAnEditor_ClosesTheEditor()
    {
        var vm = Dialog(out _);
        NetworkSettingsViewModel libera = Net(vm, "Libera");
        ServerSettingsViewModel row = libera.Servers[1];
        libera.EditServerCommand.Execute(row);

        libera.MoveServerTo(row, Net(vm, "OFTC"));

        Assert.That(row.IsEditing, Is.False);
    }
}
