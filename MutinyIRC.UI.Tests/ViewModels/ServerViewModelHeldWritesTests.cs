using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Headless.NUnit;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the settings writes of a live connection while the Networks dialog is open. They wait
/// until the dialog closes. OK applies them after the dialog, under the rules of the queue, and
/// Cancel applies them as they are.
/// </summary>
/// <remarks>
/// The view model writes settings on the UI thread only, so every test runs on it.
/// </remarks>
[TestFixture]
public class ServerViewModelHeldWritesTests
{
    private static readonly FieldInfo SingletonField =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private const string Fingerprint = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _settingsPath = null!;
    private IrcSettingsManager _settings = null!;
    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        // A file of this fixture's own: the test must never reach the user's servers.json.
        _settingsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
        _settings = new IrcSettingsManager(_settingsPath);
        SingletonField.SetValue(null, _settings);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        SingletonField.SetValue(null, null);
        File.Delete(_settingsPath);
    }

    private void Dial(string host, int port = 6667, bool ssl = false)
    {
        var args = new ConnectionArgs("me", host, ssl) { Port = port };
        _server = new Server(A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false })));
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    private void Register(string reportedNetworkName = "")
    {
        _server.Connection.Listener.Parse(":irc.fake.com 001 me :Welcome to the Internet Relay Network");
        if (reportedNetworkName != string.Empty)
            ReportNetworkName(reportedNetworkName);
    }

    private void ReportNetworkName(string name) =>
        _server.Connection.Listener.Parse(
            $":irc.fake.com 005 me NETWORK={name} CHANTYPES=# :are supported by this server");

    private NetworkSettings SaveNetwork(string name, string host, string channel, bool autoConnect = false)
    {
        NetworkSettings network = _settings.GetOrAddNetwork(name);
        network.AutoConnect = autoConnect;
        network.AddServer(new ServerSettings(host, 6667, false));
        network.AddChannel(new ChannelSettings(channel, true));
        return network;
    }

    private NetworkSettings MintNetwork(string host)
    {
        NetworkSettings minted = _settings.MintPinnedNetwork(host, 6697, Fingerprint)!;
        File.Delete(_settingsPath);
        return minted;
    }

    private NetworkSettingsViewModel Row(SettingsViewModel dialog, NetworkSettings network) =>
        dialog.Networks.Single(row => row.Source == network);

    [AvaloniaTest]
    public void ARegistrationWhileTheDialogIsOpen_DoesNotChangeTheSettingsUntilItCloses()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        var dialog = new SettingsViewModel(_settings);
        Register("HomeNet");

        Assert.Multiple(() =>
        {
            Assert.That(home.Name, Is.EqualTo("Home"));
            Assert.That(home.AutoConnect, Is.False);
        });

        dialog.Cancel();

        Assert.Multiple(() =>
        {
            Assert.That(home.Name, Is.EqualTo("HomeNet"));
            Assert.That(home.AutoConnect, Is.True);
            Assert.That(File.Exists(_settingsPath), Is.False, "Cancel must not write to disk");
        });
    }

    [AvaloniaTest]
    public void ANetworkNameReportWhileTheDialogIsOpen_WaitsUntilItCloses()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);
        Register();

        var dialog = new SettingsViewModel(_settings);
        ReportNetworkName("HomeNet");
        Assert.That(home.Name, Is.EqualTo("Home"));

        dialog.Save();

        Assert.That(home.Name, Is.EqualTo("HomeNet"));
    }

    [AvaloniaTest]
    public void APartWhileTheDialogIsOpen_WaitsUntilItCloses_ThenRemovesTheChannelByName()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);
        Register();

        var dialog = new SettingsViewModel(_settings);
        _server.Connection.Listener.Parse(":me!u@h PART #home");
        Assert.That(home.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#home" }));

        // The save replaces the channel list with new objects, so only a removal by name works.
        dialog.Save();

        Assert.That(home.Channels, Is.Empty);
    }

    [AvaloniaTest]
    public void APartWhileTheDialogIsOpen_OfAChannelTheDialogRemoved_IsDropped()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        home.AddChannel(new ChannelSettings("#other", true));
        Dial("bouncer.home");
        _vm.BindNetwork(home);
        Register();

        var dialog = new SettingsViewModel(_settings);
        _server.Connection.Listener.Parse(":me!u@h PART #home");
        NetworkSettingsViewModel row = Row(dialog, home);
        row.Channels.Remove(row.Channels.Single(c => c.Name == "#home"));
        dialog.Save();

        Assert.That(home.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#other" }));
    }

    [AvaloniaTest]
    public void WhenTheDialogDeletesTheNetwork_EveryHeldWriteForItIsDropped()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        var dialog = new SettingsViewModel(_settings);
        Register("HomeNet");
        _server.Connection.Listener.Parse(":me!u@h PART #home");
        dialog.SelectedNetwork = Row(dialog, home);
        dialog.RemoveNetworkCommand.Execute(null);
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Is.Empty);
            Assert.That(home.Name, Is.EqualTo("Home"));
            Assert.That(home.AutoConnect, Is.False);
            Assert.That(home.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#home" }));
        });
    }

    [AvaloniaTest]
    public void WhenTheDialogRenamesTheNetwork_TheHeldRenameIsDropped_AndAutoConnectStillApplies()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        var dialog = new SettingsViewModel(_settings);
        Register("HomeNet");
        Row(dialog, home).Name = "MyName";
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(home.Name, Is.EqualTo("MyName"));
            Assert.That(home.AutoConnect, Is.True);
        });
    }

    [AvaloniaTest]
    public void WhenTheDialogChangesAutoConnect_TheHeldAutoConnectIsDropped_AndTheRenameStillApplies()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home", autoConnect: true);
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        var dialog = new SettingsViewModel(_settings);
        Register("HomeNet");
        Row(dialog, home).AutoConnect = false;
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(home.AutoConnect, Is.False);
            Assert.That(home.Name, Is.EqualTo("HomeNet"));
        });
    }

    [AvaloniaTest]
    public void AHeldMergeOfAMintedNetwork_AppliesOnCancel()
    {
        NetworkSettings libera = SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        NetworkSettings minted = MintNetwork("eu.libera.chat");
        Dial("eu.libera.chat", 6697, true);
        _vm.BindNetwork(minted);

        var dialog = new SettingsViewModel(_settings);
        Register("Libera");
        Assert.That(_settings.Networks, Has.Count.EqualTo(2), "The merge waits");

        dialog.Cancel();

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks.Single(), Is.SameAs(libera));
            Assert.That(_vm.BoundNetwork, Is.SameAs(libera));
            Assert.That(_server.Channels.Keys, Does.Contain("#MutinyIRC"),
                "The connection joins the channels of the network it merged into");
        });
    }

    [AvaloniaTest]
    public void WhenTheDialogEditsTheEntryPointsOfAMintedNetwork_TheHeldMergeIsDropped()
    {
        NetworkSettings libera = SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        NetworkSettings minted = MintNetwork("eu.libera.chat");
        Dial("eu.libera.chat", 6697, true);
        _vm.BindNetwork(minted);

        var dialog = new SettingsViewModel(_settings);
        Register("Libera");
        // One entry point still, so only the edit stops the merge.
        Row(dialog, minted).Servers.Single().Port = 7000;
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Is.EqualTo(new[] { libera, minted }));
            Assert.That(minted.MintedByPin, Is.False);
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
            Assert.That(_vm.BoundNetwork, Is.SameAs(minted));
        });
    }

    [AvaloniaTest]
    public void WhenTheDialogRenamesAMintedNetwork_TheHeldMergeIsDropped()
    {
        SaveNetwork("Libera", "irc.libera.chat", "#MutinyIRC");
        NetworkSettings minted = MintNetwork("eu.libera.chat");
        Dial("eu.libera.chat", 6697, true);
        _vm.BindNetwork(minted);

        var dialog = new SettingsViewModel(_settings);
        Register("Libera");
        Row(dialog, minted).Name = "Europe";
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Has.Count.EqualTo(2));
            Assert.That(minted.MintedByPin, Is.False);
            Assert.That(minted.Name, Is.EqualTo("Europe"));
        });
    }

    [AvaloniaTest]
    public void AConnectionWithNoNetwork_WaitsForTheDialog_ThenBindsToTheNetworkTheUserAddedItsHostTo()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("irc.new.net");

        var dialog = new SettingsViewModel(_settings);
        Register();
        Assert.Multiple(() =>
        {
            Assert.That(_vm.BoundNetwork, Is.Null, "The step that finds the network waits");
            Assert.That(_settings.Networks.Single(), Is.SameAs(home));
            Assert.That(_server.Channels.Keys, Does.Not.Contain("#home"));
        });

        Row(dialog, home).Servers.Add(new ServerSettingsViewModel { Url = "irc.new.net", Port = 6667 });
        dialog.Save();

        Assert.Multiple(() =>
        {
            Assert.That(_vm.BoundNetwork, Is.SameAs(home));
            Assert.That(_settings.Networks.Single(), Is.SameAs(home), "The connection adds no network");
            Assert.That(home.Servers.Select(s => s.Url), Is.EqualTo(new[] { "bouncer.home", "irc.new.net" }));
            Assert.That(_server.Channels.Keys, Does.Contain("#home"), "It joins late, when its step runs");
        });
    }

    [AvaloniaTest]
    public void AConnectionThatWaitsForItsNetwork_UsesTheSecondNickAsItsAlternateNick()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        home.AlternateNick = "homeAlt";
        Dial("bouncer.home");

        _ = new SettingsViewModel(_settings);
        Register();

        // The session drops before the dialog closes, and the next session meets a taken nick.
        typeof(Connection).GetProperty(nameof(Connection.Registered))!
            .SetValue(_server.Connection, false);
        _server.Connection.HandleNickTaken = false;
        string? sent = null;
        ((Connection)_server.Connection).RawMessageSent += (_, e) =>
        {
            if (e.Data.StartsWith("NICK ", StringComparison.Ordinal))
                sent = e.Data;
        };
        _server.Connection.Listener.Parse(":irc.fake.com 433 me taken :Nickname is already in use");

        Assert.That(sent, Is.EqualTo("NICK " + AppSettings.Instance.SecondNick));
    }

    [AvaloniaTest]
    public void Ok_WritesTheSettingsToDiskAfterTheHeldWrites()
    {
        NetworkSettings home = SaveNetwork("Home", "bouncer.home", "#home");
        Dial("bouncer.home");
        _vm.BindNetwork(home);

        var dialog = new SettingsViewModel(_settings);
        Register("HomeNet");
        Assert.That(File.Exists(_settingsPath), Is.False, "Precondition: nothing on disk yet");
        dialog.Save();

        Assert.That(File.ReadAllText(_settingsPath), Does.Contain("\"Name\": \"HomeNet\""));
    }
}
