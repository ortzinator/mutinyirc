using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers keyboard and screen-reader access to the entry point rows of the Networks dialog:
/// the Tab order and reveal of the row buttons, F2, where the focus goes after an edit or a
/// move, the accessible names, and the live regions.
/// </summary>
[TestFixture]
public class ServerSettingsWindowAccessTests
{
    private const string Fingerprint =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _settingsFile = null!;

    [SetUp]
    public void SetUp() =>
        _settingsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void TearDown() => File.Delete(_settingsFile);

    private ServerSettingsWindow ShowWindow(out SettingsViewModel vm)
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true) { PinnedFingerprint = Fingerprint });
        libera.AddServer(new ServerSettings("eu.libera.chat", 6667, false));
        libera.AddServer(new ServerSettings("us.libera.chat", 6697, true));
        var oftc = new NetworkSettings("OFTC");
        oftc.AddServer(new ServerSettings("irc.oftc.net", 6697, true));

        // A file of this fixture's own: the window must never reach the user's servers.json.
        var manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(new[] { libera, oftc });
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

    private static Button[] RowButtons(Border row) =>
        row.GetVisualDescendants().OfType<Button>().Where(b => b.Classes.Contains("rowReveal")).ToArray();

    private static object? Focused(Window window) => window.FocusManager?.GetFocusedElement();

    private static void Press(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(key, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static (MenuFlyout Menu, MenuItem Edit, MenuItem Move) OpenMenu(Window window, int index)
    {
        Button menuButton = Named<Button>(Row(window, index), "rowMenuButton");
        var menu = (MenuFlyout)menuButton.Flyout!;
        menu.ShowAt(menuButton);
        Dispatcher.UIThread.RunJobs();
        return (menu,
            menu.Items.OfType<MenuItem>().Single(i => i.Name == "editMenuItem"),
            menu.Items.OfType<MenuItem>().Single(i => i.Name == "moveMenuItem"));
    }

    // Picks a network in the Move to submenu of a row with the keyboard, as a user does. The
    // menu handles Enter, so the click and the close of the flyout go through the real path.
    // Then the window gets the activation back from the closed popup, as on a real desktop. At
    // that time the window gives the focus again to the element that its focus scope keeps.
    private static void MoveThroughMenu(Window window, int index, string network)
    {
        Button menuButton = Named<Button>(Row(window, index), "rowMenuButton");
        menuButton.Focus();
        Press(window, PhysicalKey.Enter);
        var menu = (MenuFlyout)menuButton.Flyout!;
        MenuItem move = menu.Items.OfType<MenuItem>().Single(i => i.Name == "moveMenuItem");
        move.Focus();
        Press(window, PhysicalKey.ArrowRight);
        move.Items.OfType<MenuItem>().Single(i => (string?)i.Header == network).Focus();
        Press(window, PhysicalKey.Enter);
        Assert.That(menu.IsOpen, Is.False, "the menu closes itself after the click");

        window.Activate();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void Tab_GoesThroughTheRowButtonsInOrder()
    {
        var window = ShowWindow(out _);
        Button[] first = RowButtons(Row(window, 0));
        Button[] second = RowButtons(Row(window, 1));
        first[0].Focus();
        Dispatcher.UIThread.RunJobs();

        var reached = new object?[4];
        reached[0] = Focused(window);
        for (int i = 1; i < reached.Length; i++)
        {
            Press(window, PhysicalKey.Tab);
            reached[i] = Focused(window);
        }
        Press(window, PhysicalKey.Tab);
        object? afterRow = Focused(window);
        Press(window, PhysicalKey.Tab);
        object? nextRow = Focused(window);

        Assert.Multiple(() =>
        {
            Assert.That(reached, Is.EqualTo(first), "↑, ↓, ⋯, ✕");
            Assert.That(afterRow, Is.SameAs(Named<Button>(Row(window, 0), "forgetPinButton")),
                "the Forget link of the pin line comes after the row buttons");
            Assert.That(nextRow, Is.SameAs(second[0]), "then the next row");
        });
    }

    [AvaloniaTest]
    public void TheRowButtons_ShowWhileFocusIsInTheRow()
    {
        var window = ShowWindow(out _);
        Button[] first = RowButtons(Row(window, 0));
        Button[] second = RowButtons(Row(window, 1));
        Assert.That(first.Select(b => b.Opacity), Is.All.EqualTo(0), "hidden before focus");

        Named<Button>(Row(window, 0), "forgetPinButton").Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Multiple(() =>
        {
            Assert.That(first.Select(b => b.Opacity), Is.All.EqualTo(1));
            Assert.That(second.Select(b => b.Opacity), Is.All.EqualTo(0), "only the row with the focus");
        });
    }

    [AvaloniaTest]
    public void F2_OnARowButton_OpensTheEditor()
    {
        var window = ShowWindow(out SettingsViewModel vm);
        Named<Button>(Row(window, 1), "rowMenuButton").Focus();
        Dispatcher.UIThread.RunJobs();

        Press(window, PhysicalKey.F2);

        TextBox host = Named<TextBox>(Row(window, 1), "editHostBox");
        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectedNetwork!.Servers[1].IsEditing, Is.True);
            Assert.That(host.IsFocused, Is.True);
            Assert.That(host.SelectedText, Is.EqualTo("eu.libera.chat"));
        });
    }

    [AvaloniaTest]
    public void F2_OnThePinLine_OpensTheEditorOfThatRow()
    {
        var window = ShowWindow(out SettingsViewModel vm);
        Named<Button>(Row(window, 0), "forgetPinButton").Focus();
        Dispatcher.UIThread.RunJobs();

        Press(window, PhysicalKey.F2);

        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectedNetwork!.Servers[0].IsEditing, Is.True);
            Assert.That(Named<TextBox>(Row(window, 0), "editHostBox").IsFocused, Is.True);
        });
    }

    [AvaloniaTest]
    public void F2_InTheEditor_KeepsTheDraft()
    {
        var window = ShowWindow(out SettingsViewModel vm);
        Named<Button>(Row(window, 1), "rowMenuButton").Focus();
        Press(window, PhysicalKey.F2);
        window.KeyTextInput("x");
        Dispatcher.UIThread.RunJobs();

        Press(window, PhysicalKey.F2);

        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectedNetwork!.Servers[1].IsEditing, Is.True);
            Assert.That(Named<TextBox>(Row(window, 1), "editHostBox").Text, Is.EqualTo("x"));
        });
    }

    [AvaloniaTest]
    public void EditMenuItem_ShowsF2()
    {
        var window = ShowWindow(out _);

        (MenuFlyout menu, MenuItem edit, _) = OpenMenu(window, 0);
        menu.Hide();

        Assert.That(edit.InputGesture, Is.EqualTo(new KeyGesture(Key.F2)));
    }

    [AvaloniaTest]
    public void AfterAMove_TheFocusGoesToTheNextRow()
    {
        var window = ShowWindow(out _);

        MoveThroughMenu(window, 0, "OFTC");

        Assert.Multiple(() =>
        {
            Assert.That(Named<TextBlock>(Row(window, 0), "rowHost").Text, Is.EqualTo("eu.libera.chat"));
            Assert.That(Focused(window), Is.SameAs(Named<Button>(Row(window, 0), "rowMenuButton")));
        });
    }

    [AvaloniaTest]
    public void AfterAMoveOfTheLastRow_TheFocusGoesToThePreviousRow()
    {
        var window = ShowWindow(out _);

        MoveThroughMenu(window, 2, "OFTC");

        Assert.Multiple(() =>
        {
            Assert.That(Named<TextBlock>(Row(window, 1), "rowHost").Text, Is.EqualTo("eu.libera.chat"));
            Assert.That(Focused(window), Is.SameAs(Named<Button>(Row(window, 1), "rowMenuButton")));
        });
    }

    [AvaloniaTest]
    public void AfterAMoveOfTheOnlyRow_TheFocusGoesToTheAddBox()
    {
        var window = ShowWindow(out SettingsViewModel vm);
        vm.SelectedNetwork = vm.Networks[1];
        Dispatcher.UIThread.RunJobs();

        MoveThroughMenu(window, 0, "Libera");

        Assert.That(Focused(window), Is.SameAs(Named<TextBox>(window, "newServerBox")));
    }

    [AvaloniaTest]
    public void TheRowButtons_HaveNamesWithTheEntryPoint()
    {
        var window = ShowWindow(out _);
        Button[] buttons = RowButtons(Row(window, 0));

        Assert.That(buttons.Select(AutomationProperties.GetName), Is.EqualTo(new[]
        {
            "Try irc.libera.chat earlier",
            "Try irc.libera.chat later",
            "Actions for irc.libera.chat port 6697",
            "Remove irc.libera.chat",
        }));
    }

    [AvaloniaTest]
    public void TheForgetLink_HasANameWithTheEntryPoint()
    {
        var window = ShowWindow(out _);

        Assert.That(AutomationProperties.GetName(Named<Button>(Row(window, 0), "forgetPinButton")),
            Is.EqualTo("Forget the pinned certificate of irc.libera.chat"));
    }

    [AvaloniaTest]
    public void TheMoveToItems_HaveTheNamesOfTheirNetworks()
    {
        var window = ShowWindow(out _);

        (MenuFlyout menu, _, MenuItem move) = OpenMenu(window, 0);
        string?[] names = move.Items.OfType<MenuItem>().Select(AutomationProperties.GetName).ToArray();
        menu.Hide();

        Assert.That(names, Is.EqualTo(new[] { "OFTC" }));
    }

    [AvaloniaTest]
    public void TheEditorFields_HaveNames_AndItsLinesHaveLiveSettings()
    {
        var window = ShowWindow(out _);
        Named<Button>(Row(window, 0), "rowMenuButton").Focus();
        Press(window, PhysicalKey.F2);
        Border row = Row(window, 0);

        Assert.Multiple(() =>
        {
            Assert.That(AutomationProperties.GetName(Named<TextBox>(row, "editHostBox")), Is.EqualTo("Host"));
            Assert.That(AutomationProperties.GetName(Named<TextBox>(row, "editPortBox")), Is.EqualTo("Port"));
            Assert.That(AutomationProperties.GetHelpText(Named<TextBox>(row, "editPortBox")), Is.EqualTo("6697"));
            Assert.That(AutomationProperties.GetName(Named<CheckBox>(row, "editTlsBox")), Is.EqualTo("Use TLS"));
            Assert.That(AutomationProperties.GetLiveSetting(Named<TextBlock>(row, "editRefusal")),
                Is.EqualTo(AutomationLiveSetting.Assertive));
            Assert.That(AutomationProperties.GetLiveSetting(Named<TextBlock>(row, "pinWarning")),
                Is.EqualTo(AutomationLiveSetting.Polite));
        });
    }
}
