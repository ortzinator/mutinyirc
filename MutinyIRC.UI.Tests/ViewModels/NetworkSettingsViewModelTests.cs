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
    public void ParseServer_BareHost_TakesTheSecureDefaultPort()
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

    [Test]
    public void ParseServer_HostAndPort_TakesThePort()
    {
        var parsed = NetworkSettingsViewModel.ParseServer("irc.oftc.net:7000");

        Assert.That(parsed!.Port, Is.EqualTo(7000));
    }

    [TestCase(6667)]
    [TestCase(6665)]
    public void ParseServer_APlaintextPort_IsNotTls(int port)
    {
        // The port is the only thing we know about a host we have never dialled, so the
        // two ports that historically mean plaintext are what decide.
        var parsed = NetworkSettingsViewModel.ParseServer($"irc.libera.chat:{port}");

        Assert.That(parsed!.Ssl, Is.False);
    }

    [Test]
    public void ParseServer_AnIpAddress_Parses()
    {
        var parsed = NetworkSettingsViewModel.ParseServer("127.0.0.1:6698");

        Assert.That(parsed!.Url, Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public void ParseServer_TrimsSurroundingSpace()
    {
        Assert.That(NetworkSettingsViewModel.ParseServer("  irc.libera.chat  ")!.Url,
            Is.EqualTo("irc.libera.chat"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("irc libera chat")]
    [TestCase("irc.libera.chat:")]
    [TestCase("irc.libera.chat:port")]
    [TestCase("irc.libera.chat:6")]
    [TestCase("irc.libera.chat:999999")]
    [TestCase("irc.libera.chat:6697:6667")]
    [TestCase("irc://libera.chat")]
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
}
