using System.IO;
using System.Linq;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the Networks dialog's own state: which network is being edited, and what Save writes
/// back. The manager is injected and points at a file of this fixture's own, so nothing here
/// reads or writes the user's servers.json.
/// </summary>
[TestFixture]
public class SettingsViewModelTests
{
    private string _settingsFile = null!;

    [SetUp]
    public void SetUp() =>
        _settingsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void TearDown() => File.Delete(_settingsFile);

    private IrcSettingsManager ManagerWith(params NetworkSettings[] networks)
    {
        var manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(networks);
        return manager;
    }

    private static NetworkSettings Network(string name, string host)
    {
        var net = new NetworkSettings(name);
        net.AddServer(new ServerSettings(host, 6667, false));
        return net;
    }

    [Test]
    public void Constructor_LoadsEveryNetwork()
    {
        var vm = new SettingsViewModel(ManagerWith(
            Network("Libera", "irc.libera.chat"),
            Network("OFTC", "irc.oftc.net")));

        Assert.That(vm.Networks.Select(n => n.Name), Is.EqualTo(new[] { "Libera", "OFTC" }));
    }

    [Test]
    public void Constructor_SelectsTheFirstNetwork()
    {
        var vm = new SettingsViewModel(ManagerWith(Network("Libera", "irc.libera.chat")));

        Assert.That(vm.SelectedNetwork, Is.SameAs(vm.Networks[0]));
    }

    [Test]
    public void Constructor_NoNetworks_SelectsNothing()
    {
        var vm = new SettingsViewModel(ManagerWith());

        Assert.That(vm.SelectedNetwork, Is.Null);
    }

    [Test]
    public void AddNetwork_SelectsWhatItAdded()
    {
        var vm = new SettingsViewModel(ManagerWith());

        vm.AddNetworkCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Networks, Has.Count.EqualTo(1));
            Assert.That(vm.SelectedNetwork, Is.SameAs(vm.Networks[0]));
        });
    }

    [Test]
    public void RemoveNetwork_CannotRunWithNothingSelected()
    {
        var vm = new SettingsViewModel(ManagerWith());

        Assert.That(vm.RemoveNetworkCommand.CanExecute(null), Is.False);
    }

    [Test]
    public void RemoveNetwork_SelectsTheNeighbour()
    {
        var vm = new SettingsViewModel(ManagerWith(
            Network("Libera", "irc.libera.chat"),
            Network("OFTC", "irc.oftc.net")));

        vm.RemoveNetworkCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Networks.Select(n => n.Name), Is.EqualTo(new[] { "OFTC" }));
            Assert.That(vm.SelectedNetwork!.Name, Is.EqualTo("OFTC"));
        });
    }

    [Test]
    public void Save_WritesTheEditedNetworksBack()
    {
        IrcSettingsManager manager = ManagerWith(Network("Libera", "irc.libera.chat"));
        var vm = new SettingsViewModel(manager);
        vm.Networks[0].Name = "Libera.Chat";
        vm.Networks[0].Nick = "sparkplug";

        vm.Save();

        Assert.Multiple(() =>
        {
            Assert.That(manager.Networks.Select(n => n.Name), Is.EqualTo(new[] { "Libera.Chat" }));
            Assert.That(manager.Networks[0].Nick, Is.EqualTo("sparkplug"));
        });
    }

    [Test]
    public void Save_KeepsTheFailoverOrder()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("a.example", 6667, false));
        libera.AddServer(new ServerSettings("b.example", 6667, false));
        IrcSettingsManager manager = ManagerWith(libera);

        var vm = new SettingsViewModel(manager);
        vm.Networks[0].MoveServerDownCommand.Execute(vm.Networks[0].Servers[0]);
        vm.Save();

        Assert.That(manager.Networks[0].Servers.Select(s => s.Url),
            Is.EqualTo(new[] { "b.example", "a.example" }));
    }

    [Test]
    public void Save_DropsAHostAnEarlierNetworkAlreadyClaims()
    {
        // ReplaceAll holds the one-network-per-hostname rule, and Save is the one door the
        // settings window writes through.
        IrcSettingsManager manager = ManagerWith();
        var vm = new SettingsViewModel(manager);

        vm.AddNetworkCommand.Execute(null);
        vm.Networks[0].NewServerText = "shared.example";
        vm.Networks[0].AddServerCommand.Execute(null);

        vm.AddNetworkCommand.Execute(null);
        vm.Networks[1].NewServerText = "shared.example";
        vm.Networks[1].AddServerCommand.Execute(null);

        vm.Save();

        Assert.Multiple(() =>
        {
            Assert.That(manager.Networks[0].Servers, Has.Count.EqualTo(1));
            Assert.That(manager.Networks[1].Servers, Is.Empty);
        });
    }

    [Test]
    public void Save_WritesOnlyToTheFileItsManagerWasGiven()
    {
        // The path belongs to the manager, not to the type. A manager built for a test that
        // wrote to the shared path would overwrite the user's real servers.json.
        IrcSettingsManager manager = ManagerWith(Network("Libera", "irc.libera.chat"));

        new SettingsViewModel(manager).Save();

        Assert.That(File.Exists(_settingsFile), Is.True,
            "Save has to write the file this manager names, and nothing else");
    }

    [Test]
    public void Cancelling_LeavesTheSavedNetworksAlone()
    {
        // The window discards by closing without Save, so an edit that is never saved must
        // not have reached the manager.
        IrcSettingsManager manager = ManagerWith(Network("Libera", "irc.libera.chat"));
        var vm = new SettingsViewModel(manager);

        vm.Networks[0].Name = "Renamed";

        Assert.That(manager.Networks[0].Name, Is.EqualTo("Libera"));
    }

    // ── Pins ─────────────────────────────────────────────────────────────────

    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private IrcSettingsManager ManagerWithPinnedEntryPoint()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("a.example", 6697, true) { PinnedFingerprint = Fingerprint });
        libera.AddServer(new ServerSettings("b.example", 6697, true));
        return ManagerWith(libera);
    }

    [Test]
    public void Save_AfterForget_TheEntryPointHasNoPin()
    {
        IrcSettingsManager manager = ManagerWithPinnedEntryPoint();
        var vm = new SettingsViewModel(manager);

        vm.Networks[0].Servers[0].ForgetPinCommand.Execute(null);

        Assert.That(manager.GetCertificateTrust("a.example", 6697).PinnedFingerprint, Is.EqualTo(Fingerprint),
            "Forget waits for the save, as other edits in the dialog do");

        vm.Save();

        Assert.That(manager.GetCertificateTrust("a.example", 6697).PinnedFingerprint, Is.Null,
            "with no pin, the next connect to a self-signed server is refused");
    }

    [Test]
    public void Save_AfterAReorder_KeepsThePin()
    {
        IrcSettingsManager manager = ManagerWithPinnedEntryPoint();
        var vm = new SettingsViewModel(manager);

        vm.Networks[0].MoveServerDownCommand.Execute(vm.Networks[0].Servers[0]);
        vm.Save();

        Assert.That(manager.GetCertificateTrust("a.example", 6697).PinnedFingerprint, Is.EqualTo(Fingerprint));
    }

    [Test]
    public void Save_AfterTlsIsTurnedOff_TheEntryPointHasNoPin()
    {
        IrcSettingsManager manager = ManagerWithPinnedEntryPoint();
        var vm = new SettingsViewModel(manager);

        vm.Networks[0].Servers[0].Ssl = false;
        vm.Save();

        Assert.That(manager.GetCertificateTrust("a.example", 6697).PinnedFingerprint, Is.Null);
    }

    [Test]
    public void Save_AfterTheEntryPointIsRemovedAndAddedAgain_ItHasNoPin()
    {
        IrcSettingsManager manager = ManagerWithPinnedEntryPoint();
        var vm = new SettingsViewModel(manager);
        NetworkSettingsViewModel network = vm.Networks[0];

        network.RemoveServerCommand.Execute(network.Servers[0]);
        network.NewServerText = "a.example:6697";
        network.AddServerCommand.Execute(null);
        vm.Save();

        Assert.That(manager.GetCertificateTrust("a.example", 6697).PinnedFingerprint, Is.Null,
            "removing an entry point removes its pin");
    }

    [Test]
    public void Save_KeepsTheMintedByPinMark()
    {
        // Until a registration names a minted network, the mark is what lets that registration
        // move the entry point to the saved network of that name. A dialog save before then
        // must not clear it.
        var minted = new NetworkSettings("a.example") { MintedByPin = true };
        minted.AddServer(new ServerSettings("a.example", 6697, true) { PinnedFingerprint = Fingerprint });
        IrcSettingsManager manager = ManagerWith(minted, Network("Libera", "irc.libera.chat"));

        new SettingsViewModel(manager).Save();

        Assert.That(manager.Networks.Select(n => (n.Name, n.MintedByPin)), Is.EqualTo(new[]
        {
            ("a.example", true),
            ("Libera", false)
        }));
    }
}
