using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class NetworkSettingsViewModelTests
{
    private static NetworkSettingsViewModel WithServers(params string[] hosts)
    {
        var model = new NetworkSettings("TestNet");
        foreach (string host in hosts)
            model.AddServer(new ServerSettings(host, 6667, false));
        return new NetworkSettingsViewModel(model);
    }

    private static NetworkSettings BuildModelWithChannels(params (string name, string key)[] channels)
    {
        var model = new NetworkSettings("TestNet");
        foreach (var (name, key) in channels)
            model.AddChannel(new ChannelSettings(name, true, string.Empty, key));
        return model;
    }

    // ── Constructor / load from model ────────────────────────────────────────

    [Test]
    public void Constructor_FromModel_CopiesIdentityAndAutoConnect()
    {
        var model = new NetworkSettings("TestNet")
        {
            Nick = "sparkplug",
            AlternateNick = "sparkplug_",
            AutoConnect = true,
        };

        var vm = new NetworkSettingsViewModel(model);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Nick, Is.EqualTo("sparkplug"));
            Assert.That(vm.AlternateNick, Is.EqualTo("sparkplug_"));
            Assert.That(vm.AutoConnect, Is.True);
        });
    }

    [Test]
    public void Constructor_FromModel_NullNicksBecomeEmpty()
    {
        var vm = new NetworkSettingsViewModel(new NetworkSettings("TestNet"));

        Assert.Multiple(() =>
        {
            Assert.That(vm.Nick, Is.Empty);
            Assert.That(vm.AlternateNick, Is.Empty);
        });
    }

    [Test]
    public void Constructor_FromModel_NumbersTheServers()
    {
        var vm = WithServers("a", "b", "c");

        Assert.Multiple(() =>
        {
            Assert.That(vm.Servers.Select(s => s.Order), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(vm.Servers.Select(s => s.IsFirst), Is.EqualTo(new[] { true, false, false }));
        });
    }

    [Test]
    public void Constructor_FromModel_PopulatesChannels()
    {
        var vm = new NetworkSettingsViewModel(
            BuildModelWithChannels(("#alpha", ""), ("#beta", "k2")));

        Assert.That(vm.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#alpha", "#beta" }));
    }

    [Test]
    public void Constructor_FromModel_CopiesChannelKey()
    {
        var vm = new NetworkSettingsViewModel(BuildModelWithChannels(("#alpha", "secret")));

        Assert.That(vm.Channels[0].Key, Is.EqualTo("secret"));
    }

    [Test]
    public void Constructor_NoModel_HasEmptyCollections()
    {
        var vm = new NetworkSettingsViewModel();

        Assert.Multiple(() =>
        {
            Assert.That(vm.Servers, Is.Empty);
            Assert.That(vm.Channels, Is.Empty);
        });
    }

    // ── ParseServer ──────────────────────────────────────────────────────────

    [Test]
    public void ParseServer_BareHost_MapsTheEntryPoint()
    {
        var parsed = NetworkSettingsViewModel.ParseServer("irc.libera.chat");

        Assert.That(parsed, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(parsed!.Url, Is.EqualTo("irc.libera.chat"));
            Assert.That(parsed.Port, Is.EqualTo(6697));
            Assert.That(parsed.Ssl, Is.True);
        });
    }

    // EntryPointTests covers the parse rules. These cover only the mapping onto the view model.
    [TestCase(null)]
    [TestCase("irc libera chat")]
    public void ParseServer_Malformed_IsNull(string? text)
    {
        Assert.That(NetworkSettingsViewModel.ParseServer(text), Is.Null);
    }

    // ── AddServer ────────────────────────────────────────────────────────────

    [Test]
    public void AddServer_CannotRunUntilTheTextParses()
    {
        var vm = new NetworkSettingsViewModel();

        Assert.That(vm.AddServerCommand.CanExecute(null), Is.False);

        vm.NewServerText = "irc.libera.chat";

        Assert.That(vm.AddServerCommand.CanExecute(null), Is.True,
            "typing a usable host has to enable the button without any other prompt");
    }

    [Test]
    public void AddServer_AppendsAtTheEndOfTheOrder()
    {
        var vm = WithServers("first");
        vm.NewServerText = "second.example:6667";

        vm.AddServerCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "first", "second.example" }));
            Assert.That(vm.Servers[1].Order, Is.EqualTo(2));
        });
    }

    [Test]
    public void AddServer_ClearsTheInput()
    {
        var vm = new NetworkSettingsViewModel { NewServerText = "irc.libera.chat" };

        vm.AddServerCommand.Execute(null);

        Assert.That(vm.NewServerText, Is.Empty);
    }

    // ── RemoveServer and reordering ──────────────────────────────────────────

    [Test]
    public void RemoveServer_DropsTheRowYouPass()
    {
        var vm = WithServers("a", "b", "c");

        vm.RemoveServerCommand.Execute(vm.Servers[1]);

        Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "c" }));
    }

    [Test]
    public void RemoveServer_RenumbersWhatIsLeft()
    {
        var vm = WithServers("a", "b", "c");

        vm.RemoveServerCommand.Execute(vm.Servers[0]);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Servers.Select(s => s.Order), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(vm.Servers[0].IsFirst, Is.True, "b is now the host dialled first");
        });
    }

    [Test]
    public void MoveServerUp_SwapsWithTheRowAbove()
    {
        var vm = WithServers("a", "b", "c");

        vm.MoveServerUpCommand.Execute(vm.Servers[2]);

        Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "c", "b" }));
    }

    [Test]
    public void MoveServerDown_SwapsWithTheRowBelow()
    {
        var vm = WithServers("a", "b", "c");

        vm.MoveServerDownCommand.Execute(vm.Servers[0]);

        Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "b", "a", "c" }));
    }

    [Test]
    public void MoveServer_RenumbersAfterTheMove()
    {
        var vm = WithServers("a", "b");

        vm.MoveServerDownCommand.Execute(vm.Servers[0]);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Servers[0].Url, Is.EqualTo("b"));
            Assert.That(vm.Servers[0].IsFirst, Is.True);
            Assert.That(vm.Servers[1].IsFirst, Is.False);
        });
    }

    [Test]
    public void MoveServerUp_AtTheTop_ChangesNothing()
    {
        var vm = WithServers("a", "b");

        vm.MoveServerUpCommand.Execute(vm.Servers[0]);

        Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void MoveServerDown_AtTheBottom_ChangesNothing()
    {
        var vm = WithServers("a", "b");

        vm.MoveServerDownCommand.Execute(vm.Servers[1]);

        Assert.That(vm.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "b" }));
    }

    // ── ParseChannel ─────────────────────────────────────────────────────────

    [Test]
    public void ParseChannel_JustAName_HasNoKey()
    {
        var parsed = NetworkSettingsViewModel.ParseChannel("#mutinyirc");

        Assert.Multiple(() =>
        {
            Assert.That(parsed!.Name, Is.EqualTo("#mutinyirc"));
            Assert.That(parsed.IsKeyed, Is.False);
        });
    }

    [Test]
    public void ParseChannel_NameAndKey_SplitsOnTheSpace()
    {
        var parsed = NetworkSettingsViewModel.ParseChannel("#backstage hunter2");

        Assert.Multiple(() =>
        {
            Assert.That(parsed!.Name, Is.EqualTo("#backstage"));
            Assert.That(parsed.Key, Is.EqualTo("hunter2"));
        });
    }

    [Test]
    public void ParseChannel_KeyWithSpaces_KeepsTheWholeRest()
    {
        // Only the first space separates the two: everything after it is the key.
        Assert.That(NetworkSettingsViewModel.ParseChannel("#chan two words")!.Key,
            Is.EqualTo("two words"));
    }

    [Test]
    public void ParseChannel_NoPrefix_GetsAHash()
    {
        Assert.That(NetworkSettingsViewModel.ParseChannel("mutinyirc")!.Name,
            Is.EqualTo("#mutinyirc"));
    }

    [Test]
    public void ParseChannel_LocalPrefix_IsLeftAlone()
    {
        Assert.That(NetworkSettingsViewModel.ParseChannel("&local")!.Name, Is.EqualTo("&local"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("#")]
    [TestCase("& key")]
    public void ParseChannel_Malformed_IsNull(string? text)
    {
        Assert.That(NetworkSettingsViewModel.ParseChannel(text), Is.Null);
    }

    // ── Chips ────────────────────────────────────────────────────────────────

    [Test]
    public void AddChannel_CannotRunUntilTheTextParses()
    {
        var vm = new NetworkSettingsViewModel();

        Assert.That(vm.AddChannelCommand.CanExecute(null), Is.False);

        vm.NewChannelText = "#mutinyirc";

        Assert.That(vm.AddChannelCommand.CanExecute(null), Is.True);
    }

    [Test]
    public void AddChannel_AppendsAChip()
    {
        var vm = new NetworkSettingsViewModel { NewChannelText = "#mutinyirc" };

        vm.AddChannelCommand.Execute(null);

        Assert.That(vm.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#mutinyirc" }));
    }

    [Test]
    public void AddChannel_WithAKey_KeepsIt()
    {
        var vm = new NetworkSettingsViewModel { NewChannelText = "#backstage hunter2" };

        vm.AddChannelCommand.Execute(null);

        Assert.That(vm.Channels[0].IsKeyed, Is.True);
    }

    [Test]
    public void AddChannel_ClosesTheInput()
    {
        var vm = new NetworkSettingsViewModel();
        vm.BeginAddChannelCommand.Execute(null);
        vm.NewChannelText = "#mutinyirc";

        vm.AddChannelCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsAddingChannel, Is.False);
            Assert.That(vm.NewChannelText, Is.Empty);
        });
    }

    [Test]
    public void BeginAddChannel_OpensTheInputEmpty()
    {
        var vm = new NetworkSettingsViewModel { NewChannelText = "leftover" };

        vm.BeginAddChannelCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsAddingChannel, Is.True);
            Assert.That(vm.NewChannelText, Is.Empty);
        });
    }

    [Test]
    public void CancelAddChannel_ClosesTheInputAndAddsNothing()
    {
        var vm = new NetworkSettingsViewModel();
        vm.BeginAddChannelCommand.Execute(null);
        vm.NewChannelText = "#mutinyirc";

        vm.CancelAddChannelCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsAddingChannel, Is.False);
            Assert.That(vm.Channels, Is.Empty);
        });
    }

    [Test]
    public void RemoveChannel_DropsTheChipYouPass()
    {
        var vm = new NetworkSettingsViewModel(
            BuildModelWithChannels(("#alpha", ""), ("#beta", "")));

        vm.RemoveChannelCommand.Execute(vm.Channels[0]);

        Assert.That(vm.Channels.Select(c => c.Name), Is.EqualTo(new[] { "#beta" }));
    }

    // ── Rename ───────────────────────────────────────────────────────────────

    [Test]
    public void CommitEdit_TakesTheEditedName()
    {
        var vm = new NetworkSettingsViewModel { Name = "Old" };
        vm.BeginEdit();
        vm.EditName = "New";

        vm.CommitEditCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Name, Is.EqualTo("New"));
            Assert.That(vm.IsEditing, Is.False);
        });
    }

    [Test]
    public void CancelEdit_KeepsTheOldName()
    {
        var vm = new NetworkSettingsViewModel { Name = "Old" };
        vm.BeginEdit();
        vm.EditName = "New";

        vm.CancelEditCommand.Execute(null);

        Assert.That(vm.Name, Is.EqualTo("Old"));
    }

    // ── ToModel ──────────────────────────────────────────────────────────────

    [Test]
    public void ToModel_KeepsTheFailoverOrder()
    {
        var vm = WithServers("a", "b", "c");
        vm.MoveServerUpCommand.Execute(vm.Servers[2]);

        NetworkSettings model = vm.ToModel();

        Assert.That(model.Servers.Select(s => s.Url), Is.EqualTo(new[] { "a", "c", "b" }));
    }

    [Test]
    public void ToModel_BlankNicks_BecomeNullSoTheAppDefaultsApply()
    {
        var vm = new NetworkSettingsViewModel { Nick = "  ", AlternateNick = string.Empty };

        NetworkSettings model = vm.ToModel();

        Assert.Multiple(() =>
        {
            Assert.That(model.Nick, Is.Null);
            Assert.That(model.AlternateNick, Is.Null);
        });
    }

    [Test]
    public void ToModel_EveryChannelAutoJoins()
    {
        var vm = new NetworkSettingsViewModel { NewChannelText = "#mutinyirc" };
        vm.AddChannelCommand.Execute(null);

        Assert.That(vm.ToModel().Channels[0].AutoJoin, Is.True);
    }

    [Test]
    public void ToModel_CarriesAutoConnectThrough()
    {
        // Nothing in this dialog can change the flag yet, so a round trip must not lose
        // the value the saved file already holds.
        var vm = new NetworkSettingsViewModel { AutoConnect = true };

        Assert.That(vm.ToModel().AutoConnect, Is.True);
    }

    // ── Verify TLS certificates ──────────────────────────────────────────────

    private static NetworkSettingsViewModel WithTlsServer(bool verify)
    {
        var model = new NetworkSettings("TestNet") { VerifyCertificates = verify };
        model.AddServer(new ServerSettings("irc.example.net", 6697, true));
        return new NetworkSettingsViewModel(model);
    }

    [Test]
    public void ToModel_KeepsEveryPin()
    {
        // Saving the Networks dialog writes the networks it rebuilt, so a pin that the dialog
        // does not carry through is lost.
        var model = new NetworkSettings("TestNet");
        model.AddServer(new ServerSettings("a.example.net", 6697, true) { PinnedFingerprint = new string('A', 64) });
        model.AddServer(new ServerSettings("b.example.net", 6697, true));
        model.AddServer(new ServerSettings("c.example.net", 6697, true) { PinnedFingerprint = new string('C', 64) });
        var vm = new NetworkSettingsViewModel(model);
        vm.MoveServerUpCommand.Execute(vm.Servers[2]);

        NetworkSettings saved = vm.ToModel();

        Assert.That(saved.Servers.Select(s => (s.Url, s.PinnedFingerprint)), Is.EqualTo(new[]
        {
            ("a.example.net", new string('A', 64)),
            ("c.example.net", new string('C', 64)),
            ("b.example.net", null)
        }));
    }

    [Test]
    public void VerifySwitch_RoundTripsThroughTheModel()
    {
        NetworkSettingsViewModel vm = WithTlsServer(verify: false);

        Assert.That(vm.VerifyCertificates, Is.False);
        vm.VerifyCertificates = true;
        Assert.That(vm.ToModel().VerifyCertificates, Is.True);
        vm.VerifyCertificates = false;
        Assert.That(vm.ToModel().VerifyCertificates, Is.False);
    }

    [Test]
    public void ANewNetwork_Verifies()
    {
        Assert.That(new NetworkSettingsViewModel().VerifyCertificates, Is.True);
    }

    [Test]
    public void VerifySwitchOn_DescribesTheCheck()
    {
        NetworkSettingsViewModel vm = WithTlsServer(verify: true);

        Assert.Multiple(() =>
        {
            Assert.That(vm.CanVerifyCertificates, Is.True);
            Assert.That(vm.VerifyCertificatesHint,
                Is.EqualTo("Refuses a server whose certificate fails the check, unless you pinned that certificate"));
            Assert.That(vm.IsVerifyCertificatesHintWarning, Is.False);
        });
    }

    [Test]
    public void VerifySwitchOff_WarnsThatAnyCertificateIsAccepted()
    {
        NetworkSettingsViewModel vm = WithTlsServer(verify: false);

        Assert.Multiple(() =>
        {
            Assert.That(vm.VerifyCertificatesHint,
                Is.EqualTo("Off: Mutiny accepts any certificate from this network's servers"));
            Assert.That(vm.IsVerifyCertificatesHintWarning, Is.True);
        });
    }

    [Test]
    public void ANetworkWithNoTlsServer_CannotVerify()
    {
        NetworkSettingsViewModel vm = WithServers("irc.example.net");
        vm.VerifyCertificates = false;

        Assert.Multiple(() =>
        {
            Assert.That(vm.CanVerifyCertificates, Is.False);
            Assert.That(vm.VerifyCertificatesHint,
                Is.EqualTo("None of this network's servers use TLS, so there is no certificate to check."));
            Assert.That(vm.IsVerifyCertificatesHintWarning, Is.False,
                "a switch that does nothing has nothing to warn about");
        });
    }

    [Test]
    public void AddingATlsServer_EnablesTheSwitch()
    {
        NetworkSettingsViewModel vm = WithServers("irc.example.net");
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.NewServerText = "tls.example.net:6697";
        vm.AddServerCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(vm.CanVerifyCertificates, Is.True);
            Assert.That(changed, Does.Contain(nameof(NetworkSettingsViewModel.CanVerifyCertificates)));
            Assert.That(changed, Does.Contain(nameof(NetworkSettingsViewModel.VerifyCertificatesHint)));
        });
    }

    [Test]
    public void RemovingTheLastTlsServer_DisablesTheSwitch()
    {
        NetworkSettingsViewModel vm = WithTlsServer(verify: true);

        vm.RemoveServerCommand.Execute(vm.Servers[0]);

        Assert.That(vm.CanVerifyCertificates, Is.False);
    }
}
