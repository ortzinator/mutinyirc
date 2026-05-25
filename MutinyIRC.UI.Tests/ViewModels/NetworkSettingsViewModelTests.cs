using System.Linq;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class NetworkSettingsViewModelTests
{
    private static NetworkSettings BuildModelWithChannels(params (string name, bool autoJoin, string key)[] channels)
    {
        var model = new NetworkSettings("TestNet");
        foreach (var (name, autoJoin, key) in channels)
            model.AddChannel(new ChannelSettings(name, autoJoin, string.Empty, key));
        return model;
    }

    // ── Constructor / load from model ────────────────────────────────────────

    [Test]
    public void Constructor_FromModel_PopulatesChannels()
    {
        var model = BuildModelWithChannels(
            ("#alpha", true, ""),
            ("#beta", false, "k2"));

        var vm = new NetworkSettingsViewModel(model);

        Assert.That(vm.Channels.Count, Is.EqualTo(2));
    }

    [Test]
    public void Constructor_FromModel_PreservesChannelOrder()
    {
        var model = BuildModelWithChannels(
            ("#alpha", true, ""),
            ("#beta", false, "k2"),
            ("#gamma", true, "k3"));

        var vm = new NetworkSettingsViewModel(model);

        Assert.That(vm.Channels.Select(c => c.Name),
            Is.EqualTo(new[] { "#alpha", "#beta", "#gamma" }));
    }

    [Test]
    public void Constructor_FromModel_CopiesChannelKey()
    {
        var model = BuildModelWithChannels(("#alpha", true, "secret"));
        var vm = new NetworkSettingsViewModel(model);
        Assert.That(vm.Channels[0].Key, Is.EqualTo("secret"));
    }

    [Test]
    public void Constructor_NoModel_HasEmptyChannels()
    {
        var vm = new NetworkSettingsViewModel();
        Assert.That(vm.Channels, Is.Empty);
    }

    // ── AddChannel ───────────────────────────────────────────────────────────

    [Test]
    public void AddChannel_AppendsToCollection()
    {
        var vm = new NetworkSettingsViewModel();
        vm.AddChannelCommand.Execute(null);
        Assert.That(vm.Channels.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddChannel_SelectsNewItem()
    {
        var vm = new NetworkSettingsViewModel();
        vm.AddChannelCommand.Execute(null);
        Assert.That(vm.SelectedChannel, Is.SameAs(vm.Channels[0]));
    }

    [Test]
    public void AddChannel_DefaultsAutoJoinTrue()
    {
        var vm = new NetworkSettingsViewModel();
        vm.AddChannelCommand.Execute(null);
        Assert.That(vm.SelectedChannel!.AutoJoin, Is.True);
    }

    [Test]
    public void AddChannel_TwiceAppendsTwo()
    {
        var vm = new NetworkSettingsViewModel();
        vm.AddChannelCommand.Execute(null);
        vm.AddChannelCommand.Execute(null);
        Assert.That(vm.Channels.Count, Is.EqualTo(2));
    }

    // ── RemoveChannel ────────────────────────────────────────────────────────

    [Test]
    public void RemoveChannel_RemovesSelectedItem()
    {
        var model = BuildModelWithChannels(
            ("#a", true, ""),
            ("#b", true, ""),
            ("#c", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = vm.Channels[1];

        vm.RemoveChannelCommand.Execute(null);

        Assert.That(vm.Channels.Select(c => c.Name),
            Is.EqualTo(new[] { "#a", "#c" }));
    }

    [Test]
    public void RemoveChannel_MidList_SelectsNextAtSameIndex()
    {
        var model = BuildModelWithChannels(
            ("#a", true, ""),
            ("#b", true, ""),
            ("#c", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = vm.Channels[1];

        vm.RemoveChannelCommand.Execute(null);

        Assert.That(vm.SelectedChannel!.Name, Is.EqualTo("#c"));
    }

    [Test]
    public void RemoveChannel_LastItem_SelectsPrevious()
    {
        var model = BuildModelWithChannels(
            ("#a", true, ""),
            ("#b", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = vm.Channels[1];

        vm.RemoveChannelCommand.Execute(null);

        Assert.That(vm.SelectedChannel!.Name, Is.EqualTo("#a"));
    }

    [Test]
    public void RemoveChannel_OnlyItem_ClearsSelection()
    {
        var model = BuildModelWithChannels(("#a", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = vm.Channels[0];

        vm.RemoveChannelCommand.Execute(null);

        Assert.That(vm.SelectedChannel, Is.Null);
        Assert.That(vm.Channels, Is.Empty);
    }

    [Test]
    public void RemoveChannelCommand_DisabledWhenNoSelection()
    {
        var model = BuildModelWithChannels(("#a", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = null;

        Assert.That(vm.RemoveChannelCommand.CanExecute(null), Is.False);
    }

    [Test]
    public void RemoveChannelCommand_EnabledWhenItemSelected()
    {
        var model = BuildModelWithChannels(("#a", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = vm.Channels[0];

        Assert.That(vm.RemoveChannelCommand.CanExecute(null), Is.True);
    }

    [Test]
    public void RemoveChannelCommand_RaisesCanExecuteChangedOnSelectionChange()
    {
        var model = BuildModelWithChannels(("#a", true, ""));
        var vm = new NetworkSettingsViewModel(model);
        vm.SelectedChannel = null;

        int fired = 0;
        vm.RemoveChannelCommand.CanExecuteChanged += (_, _) => fired++;

        vm.SelectedChannel = vm.Channels[0];

        Assert.That(fired, Is.GreaterThan(0));
    }

    // ── ToModel: regression for dropped-channels bug ─────────────────────────

    [Test]
    public void ToModel_IncludesChannels()
    {
        var vm = new NetworkSettingsViewModel { Name = "Net" };
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#x", Key = "k", AutoJoin = true });

        var model = vm.ToModel();

        Assert.That(model.Channels.Count, Is.EqualTo(1));
    }

    [Test]
    public void ToModel_PreservesChannelName()
    {
        var vm = new NetworkSettingsViewModel { Name = "Net" };
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#x", Key = "k", AutoJoin = true });

        var model = vm.ToModel();

        Assert.That(model.Channels[0].Name, Is.EqualTo("#x"));
    }

    [Test]
    public void ToModel_PreservesChannelKey()
    {
        var vm = new NetworkSettingsViewModel { Name = "Net" };
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#x", Key = "secret", AutoJoin = true });

        var model = vm.ToModel();

        Assert.That(model.Channels[0].Key, Is.EqualTo("secret"));
    }

    [Test]
    public void ToModel_PreservesChannelAutoJoin()
    {
        var vm = new NetworkSettingsViewModel { Name = "Net" };
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#x", Key = "", AutoJoin = false });

        var model = vm.ToModel();

        Assert.That(model.Channels[0].AutoJoin, Is.False);
    }

    [Test]
    public void ToModel_PreservesChannelOrder()
    {
        var vm = new NetworkSettingsViewModel { Name = "Net" };
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#first" });
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#second" });
        vm.Channels.Add(new ChannelSettingsViewModel { Name = "#third" });

        var model = vm.ToModel();

        Assert.That(model.Channels.Select(c => c.Name),
            Is.EqualTo(new[] { "#first", "#second", "#third" }));
    }

    [Test]
    public void ToModel_RoundTripsChannelsThroughVm()
    {
        var original = BuildModelWithChannels(
            ("#alpha", true, "k1"),
            ("#beta", false, ""));
        var vm = new NetworkSettingsViewModel(original);

        var roundTripped = vm.ToModel();

        Assert.That(roundTripped.Channels.Count, Is.EqualTo(2));
        Assert.That(roundTripped.Channels[0].Name, Is.EqualTo("#alpha"));
        Assert.That(roundTripped.Channels[0].Key, Is.EqualTo("k1"));
        Assert.That(roundTripped.Channels[0].AutoJoin, Is.True);
        Assert.That(roundTripped.Channels[1].Name, Is.EqualTo("#beta"));
        Assert.That(roundTripped.Channels[1].AutoJoin, Is.False);
    }
}