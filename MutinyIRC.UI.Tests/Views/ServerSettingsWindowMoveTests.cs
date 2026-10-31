using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers the Move to submenu of an entry point row in the Networks dialog, and the status line
/// that a move shows under the entry point list.
/// </summary>
[TestFixture]
public class ServerSettingsWindowMoveTests
{
    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _settingsFile = null!;

    [SetUp]
    public void SetUp() =>
        _settingsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void TearDown() => File.Delete(_settingsFile);

    private ServerSettingsWindow ShowWindow(out SettingsViewModel vm, out IrcSettingsManager manager)
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true) { PinnedFingerprint = Fingerprint });
        libera.AddServer(new ServerSettings("eu.libera.chat", 6667, false));
        var oftc = new NetworkSettings("OFTC");
        oftc.AddServer(new ServerSettings("irc.oftc.net", 6697, true));
        var rizon = new NetworkSettings("Rizon");

        // A file of this fixture's own: the window must never reach the user's servers.json.
        manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(new[] { libera, oftc, rizon });
        vm = new SettingsViewModel(manager);

        var window = new ServerSettingsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Border Row(Window window, int index) =>
        window.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.Classes.Contains("serverRow"))
            .ElementAt(index);

    private static T Named<T>(Visual root, string name) where T : Control =>
        root.GetVisualDescendants().OfType<T>().Single(c => c.Name == name);

    // Opens the ⋯ menu of a row and gets its Move to item. The caller closes the menu.
    private static (MenuFlyout Menu, MenuItem Move) OpenMoveMenu(Window window, int index)
    {
        Button menuButton = Named<Button>(Row(window, index), "rowMenuButton");
        var menu = (MenuFlyout)menuButton.Flyout!;
        menu.ShowAt(menuButton);
        Dispatcher.UIThread.RunJobs();
        return (menu, menu.Items.OfType<MenuItem>().Single(i => i.Name == "moveMenuItem"));
    }

    // Picks a network in the Move to submenu of a row, as a user does.
    private static void MoveThroughMenu(Window window, int index, string network)
    {
        (MenuFlyout menu, MenuItem move) = OpenMoveMenu(window, index);
        MenuItem target = move.Items.OfType<MenuItem>().Single(i => (string?)i.Header == network);
        target.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        menu.Hide();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void MoveTo_ListsEveryNetworkExceptTheCurrentOne()
    {
        var window = ShowWindow(out _, out _);

        (MenuFlyout menu, MenuItem move) = OpenMoveMenu(window, 0);
        string?[] names = move.Items.OfType<MenuItem>().Select(i => (string?)i.Header).ToArray();
        menu.Hide();

        Assert.Multiple(() =>
        {
            Assert.That(move.Header, Is.EqualTo("Move to"));
            Assert.That(move.IsEnabled, Is.True);
            Assert.That(names, Is.EqualTo(new[] { "OFTC", "Rizon" }));
        });
    }

    [AvaloniaTest]
    public void MoveTo_FollowsANetworkAddedWhileTheDialogIsOpen()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);
        vm.AddNetworkCommand.Execute(null);
        vm.SelectedNetwork = vm.Networks[0];
        Dispatcher.UIThread.RunJobs();

        (MenuFlyout menu, MenuItem move) = OpenMoveMenu(window, 0);
        string?[] names = move.Items.OfType<MenuItem>().Select(i => (string?)i.Header).ToArray();
        menu.Hide();

        Assert.That(names, Is.EqualTo(new[] { "OFTC", "Rizon", "New Network" }));
    }

    [AvaloniaTest]
    public void PickingANetwork_MovesTheEntryPointAndShowsTheStatusLine()
    {
        var window = ShowWindow(out SettingsViewModel vm, out IrcSettingsManager manager);

        MoveThroughMenu(window, 0, "OFTC");

        TextBlock status = Named<TextBlock>(window, "moveStatus");
        Assert.Multiple(() =>
        {
            Assert.That(vm.Networks[0].Servers.Select(s => s.Url), Is.EqualTo(new[] { "eu.libera.chat" }));
            Assert.That(vm.Networks[1].Servers.Select(s => s.Url),
                Is.EqualTo(new[] { "irc.oftc.net", "irc.libera.chat" }));
            Assert.That(window.GetVisualDescendants().OfType<Border>().Count(b => b.Classes.Contains("serverRow")),
                Is.EqualTo(1), "the row leaves the shown network");
            Assert.That(status.IsEffectivelyVisible, Is.True);
            Assert.That(status.Text, Is.EqualTo("Moved irc.libera.chat to OFTC."));
            Assert.That(AutomationProperties.GetLiveSetting(status), Is.EqualTo(AutomationLiveSetting.Polite));
            Assert.That(window.IsVisible, Is.True, "a move does not close the dialog");
            Assert.That(manager.Networks[1].Servers, Has.Count.EqualTo(1), "a move is saved only by Save");
        });

        vm.Save();
        Assert.Multiple(() =>
        {
            Assert.That(manager.Networks[1].Servers.Select(s => s.Url),
                Is.EqualTo(new[] { "irc.oftc.net", "irc.libera.chat" }));
            Assert.That(manager.Networks[1].Servers[1].PinnedFingerprint, Is.EqualTo(Fingerprint));
        });
    }

    [AvaloniaTest]
    public void MovingTheLastEntryPoint_KeepsTheNetworkInTheRail()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);

        MoveThroughMenu(window, 0, "Rizon");
        MoveThroughMenu(window, 0, "Rizon");

        Assert.Multiple(() =>
        {
            Assert.That(vm.Networks.Select(n => n.Name), Is.EqualTo(new[] { "Libera", "OFTC", "Rizon" }));
            Assert.That(vm.SelectedNetwork, Is.SameAs(vm.Networks[0]));
            Assert.That(vm.SelectedNetwork!.Servers, Is.Empty);
            Assert.That(Named<TextBlock>(window, "moveStatus").Text, Is.EqualTo("Moved eu.libera.chat to Rizon."));
        });
    }

    [AvaloniaTest]
    public void TheStatusLine_ClearsAtTheNextActionInTheNetwork()
    {
        var window = ShowWindow(out _, out _);
        MoveThroughMenu(window, 1, "OFTC");
        TextBlock status = Named<TextBlock>(window, "moveStatus");
        Assert.That(status.IsEffectivelyVisible, Is.True);

        Named<TextBox>(window, "newServerBox").Text = "irc.example.net";
        Dispatcher.UIThread.RunJobs();

        Assert.That(status.IsEffectivelyVisible, Is.False);
    }

    [AvaloniaTest]
    public void WithOneNetwork_MoveToIsDisabled()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true));
        var manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(new[] { libera });
        var window = new ServerSettingsWindow(new SettingsViewModel(manager));
        window.Show();
        Dispatcher.UIThread.RunJobs();

        (MenuFlyout menu, MenuItem move) = OpenMoveMenu(window, 0);
        menu.Hide();

        Assert.Multiple(() =>
        {
            Assert.That(move.IsEnabled, Is.False);
            Assert.That(move.Items, Is.Empty);
        });
    }
}
