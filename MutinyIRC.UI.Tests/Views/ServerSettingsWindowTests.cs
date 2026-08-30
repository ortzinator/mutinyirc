using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers what the Networks dialog draws for a network: its entry points in order, the three
/// connection switches that are drawn but not yet wired up, and the autojoin chips.
/// </summary>
[TestFixture]
public class ServerSettingsWindowTests
{
    private static SettingsViewModel BuildViewModel()
    {
        var libera = new NetworkSettings("Libera") { Nick = "sparkplug" };
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true));
        libera.AddServer(new ServerSettings("eu.libera.chat", 6667, false));
        libera.AddChannel(new Common.ChannelSettings("#mutinyirc", true));
        libera.AddChannel(new Common.ChannelSettings("#backstage", true, string.Empty, "hunter2"));

        // A file of this fixture's own: the window must never reach the user's servers.json.
        var manager = new IrcSettingsManager(
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
        manager.Networks.ReplaceAll(new[] { libera });
        return new SettingsViewModel(manager);
    }

    private static ServerSettingsWindow ShowWindow(out SettingsViewModel vm)
    {
        vm = BuildViewModel();
        var window = new ServerSettingsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    [AvaloniaTest]
    public void TheThreeConnectionSwitches_AreDrawnAndDisabled()
    {
        var window = ShowWindow(out _);

        var toggles = window.GetVisualDescendants().OfType<ToggleSwitch>().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(toggles, Has.Count.EqualTo(3),
                "the dialog draws all three settings even though none of them is wired up");
            Assert.That(toggles.All(t => !t.IsEffectivelyEnabled), Is.True,
                "a switch that writes nothing must not look as if it does");
        });
    }

    [AvaloniaTest]
    public void EveryServer_GetsARow()
    {
        var window = ShowWindow(out _);

        var rows = window.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.Classes.Contains("serverRow"))
            .ToList();

        Assert.That(rows, Has.Count.EqualTo(2));
    }

    [AvaloniaTest]
    public void TheFirstServer_IsMarkedAsTheOneDialledFirst()
    {
        var window = ShowWindow(out _);

        var orders = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => t.Classes.Contains("order"))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(orders.Select(o => o.Text), Is.EqualTo(new[] { "1", "2" }));
            Assert.That(orders[0].Classes.Contains("first"), Is.True,
                "the top of the failover order has to be visible without counting rows");
            Assert.That(orders[1].Classes.Contains("first"), Is.False);
        });
    }

    [AvaloniaTest]
    public void EachServerRow_ShowsItsOwnPort()
    {
        var window = ShowWindow(out _);

        var texts = window.GetVisualDescendants().OfType<TextBlock>()
            .Select(t => t.Text).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(texts, Does.Contain("6697"));
            Assert.That(texts, Does.Contain("6667"),
                "one port per entry point is the whole point of the new table");
        });
    }

    [AvaloniaTest]
    public void AKeyedChannel_CarriesThePadlock()
    {
        var window = ShowWindow(out _);

        var padlocks = window.GetVisualDescendants()
            .OfType<SymbolIcon>()
            .Where(i => i.Symbol == Symbol.LockClosed)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(padlocks, Has.Count.EqualTo(2), "one padlock is drawn per chip");
            Assert.That(padlocks.Count(p => p.IsVisible), Is.EqualTo(1),
                "only the channel that needs a key shows one");
        });
    }

    [AvaloniaTest]
    public void TheAddServerButton_EnablesOnlyOnAHostItCanRead()
    {
        var window = ShowWindow(out SettingsViewModel vm);
        var addButton = window.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Name == "addServerButton");

        Assert.That(addButton.IsEffectivelyEnabled, Is.False,
            "there is nothing to add before anything is typed");

        vm.SelectedNetwork!.NewServerText = "irc.oftc.net:6697";
        Dispatcher.UIThread.RunJobs();

        Assert.That(addButton.IsEffectivelyEnabled, Is.True);

        vm.SelectedNetwork.NewServerText = "not a host";
        Dispatcher.UIThread.RunJobs();

        Assert.That(addButton.IsEffectivelyEnabled, Is.False,
            "input the parser rejects must leave the button dead rather than add a bad host");
    }

    [AvaloniaTest]
    public void SelectingAnotherNetwork_RedrawsItsServers()
    {
        var window = ShowWindow(out SettingsViewModel vm);

        vm.AddNetworkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var rows = window.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.Classes.Contains("serverRow"));

        Assert.That(rows, Is.Empty, "a network with no entry points draws no rows");
    }
}
