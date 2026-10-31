using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Covers the editor that an entry point row of the Networks dialog becomes: how it opens, what
/// it draws, and how Enter and Escape in it work with the Save (<c>IsDefault</c>) and Cancel
/// (<c>IsCancel</c>) buttons of the dialog.
/// </summary>
[TestFixture]
public class ServerSettingsWindowEditTests
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

        // A file of this fixture's own: the window must never reach the user's servers.json.
        manager = new IrcSettingsManager(_settingsFile);
        manager.Networks.ReplaceAll(new[] { libera });
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

    private static void Press(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(key, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    // Opens the editor of a row through its ⋯ menu, as a user does.
    private static void OpenThroughMenu(Window window, int index)
    {
        Button menuButton = Named<Button>(Row(window, index), "rowMenuButton");
        var menu = (MenuFlyout)menuButton.Flyout!;
        menu.ShowAt(menuButton);
        Dispatcher.UIThread.RunJobs();

        MenuItem edit = menu.Items.OfType<MenuItem>().Single(i => i.Name == "editMenuItem");
        edit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        menu.Hide();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void EditInTheRowMenu_OpensTheEditorWithTheCaretInTheHost()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);

        OpenThroughMenu(window, 0);

        ServerSettingsViewModel row = vm.SelectedNetwork!.Servers[0];
        TextBox host = Named<TextBox>(Row(window, 0), "editHostBox");
        Assert.Multiple(() =>
        {
            Assert.That(row.IsEditing, Is.True);
            Assert.That(host.IsEffectivelyVisible, Is.True);
            Assert.That(host.Text, Is.EqualTo("irc.libera.chat"));
            Assert.That(host.IsFocused, Is.True, "the caret goes to the host field");
            Assert.That(host.SelectedText, Is.EqualTo("irc.libera.chat"), "the host is selected");
            Assert.That(Named<TextBox>(Row(window, 0), "editPortBox").Text, Is.EqualTo("6697"));
            Assert.That(Named<CheckBox>(Row(window, 0), "editTlsBox").IsChecked, Is.True);
            Assert.That(Named<StackPanel>(Row(window, 0), "pinLine").IsEffectivelyVisible, Is.True,
                "the pin line stays under the row while you edit");
            Assert.That(vm.SelectedNetwork.Servers[1].IsEditing, Is.False, "only that row opens");
        });
    }

    [AvaloniaTest]
    public void ADoubleClickOnTheRow_OpensTheEditor()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);
        Border row = Row(window, 1);
        Point onHost = row.TranslatePoint(new Point(row.Bounds.Width / 2, 12), window)!.Value;

        window.MouseDown(onHost, MouseButton.Left);
        window.MouseUp(onHost, MouseButton.Left);
        window.MouseDown(onHost, MouseButton.Left);
        window.MouseUp(onHost, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.SelectedNetwork!.Servers[1].IsEditing, Is.True);
    }

    [AvaloniaTest]
    public void Enter_AppliesTheEditAndDoesNotSaveTheDialog()
    {
        var window = ShowWindow(out SettingsViewModel vm, out IrcSettingsManager manager);
        OpenThroughMenu(window, 1);

        window.KeyTextInput("us.libera.chat");
        Dispatcher.UIThread.RunJobs();
        Press(window, PhysicalKey.Enter);

        ServerSettingsViewModel row = vm.SelectedNetwork!.Servers[1];
        Assert.Multiple(() =>
        {
            Assert.That(row.IsEditing, Is.False, "Enter applies and closes the editor");
            Assert.That(row.Url, Is.EqualTo("us.libera.chat"));
            Assert.That(window.IsVisible, Is.True, "Enter must not reach the Save button");
            Assert.That(manager.Networks[0].Servers.Select(s => s.Url),
                Does.Not.Contain("us.libera.chat"), "the dialog is not saved");
            Assert.That(Named<Button>(Row(window, 1), "rowMenuButton").IsFocused, Is.True,
                "the focus goes back to the row");
        });
    }

    [AvaloniaTest]
    public void Enter_WhileARefusalDisablesApply_DoesNothing()
    {
        var window = ShowWindow(out SettingsViewModel vm, out IrcSettingsManager manager);
        OpenThroughMenu(window, 1);

        window.KeyTextInput("IRC.libera.chat");
        Dispatcher.UIThread.RunJobs();

        Border rowControl = Row(window, 1);
        TextBlock refusal = Named<TextBlock>(rowControl, "editRefusal");
        Assert.Multiple(() =>
        {
            Assert.That(refusal.IsEffectivelyVisible, Is.True);
            Assert.That(refusal.Text, Is.EqualTo("IRC.libera.chat is already listed in this network."));
            Assert.That(Named<Button>(rowControl, "applyEditButton").IsEffectivelyEnabled, Is.False);
        });

        Press(window, PhysicalKey.Enter);

        ServerSettingsViewModel row = vm.SelectedNetwork!.Servers[1];
        Assert.Multiple(() =>
        {
            Assert.That(row.IsEditing, Is.True, "the editor stays open");
            Assert.That(row.Url, Is.EqualTo("eu.libera.chat"), "nothing is applied");
            Assert.That(window.IsVisible, Is.True, "Enter must not reach the Save button");
            Assert.That(manager.Networks[0].Servers.Select(s => s.Url),
                Is.EqualTo(new[] { "irc.libera.chat", "eu.libera.chat" }), "the dialog is not saved");
        });
    }

    [AvaloniaTest]
    public void Escape_CancelsTheEdit_AndASecondEscapeClosesTheDialog()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);
        bool closed = false;
        window.Closed += (_, _) => closed = true;
        OpenThroughMenu(window, 0);
        window.KeyTextInput("us.libera.chat");
        Dispatcher.UIThread.RunJobs();

        Press(window, PhysicalKey.Escape);

        ServerSettingsViewModel row = vm.SelectedNetwork!.Servers[0];
        Assert.Multiple(() =>
        {
            Assert.That(row.IsEditing, Is.False, "Escape cancels");
            Assert.That(row.Url, Is.EqualTo("irc.libera.chat"), "the row keeps its values");
            Assert.That(row.PinnedFingerprint, Is.EqualTo(Fingerprint));
            Assert.That(closed, Is.False, "the first Escape must not reach the Cancel button");
        });

        Press(window, PhysicalKey.Escape);

        Assert.That(closed, Is.True, "a second Escape closes the dialog");
    }

    [AvaloniaTest]
    public void TheTlsCheckBox_ChangesThePlaceholderAndNeverThePort()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);
        OpenThroughMenu(window, 0);
        Border rowControl = Row(window, 0);
        TextBox port = Named<TextBox>(rowControl, "editPortBox");

        Named<CheckBox>(rowControl, "editTlsBox").IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        Assert.That(port.Text, Is.EqualTo("6697"), "TLS off keeps the port");

        port.Text = string.Empty;
        Dispatcher.UIThread.RunJobs();
        Assert.That(port.PlaceholderText, Is.EqualTo("6667"));

        Named<CheckBox>(rowControl, "editTlsBox").IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Multiple(() =>
        {
            Assert.That(port.PlaceholderText, Is.EqualTo("6697"));
            Assert.That(vm.SelectedNetwork!.Servers[0].Ssl, Is.True, "the row changes only on Apply");
        });
    }

    [AvaloniaTest]
    public void ThePinWarning_ShowsForTlsOff()
    {
        var window = ShowWindow(out _, out _);
        OpenThroughMenu(window, 0);
        Border rowControl = Row(window, 0);
        TextBlock warning = Named<TextBlock>(rowControl, "pinWarning");
        Assert.That(warning.IsEffectivelyVisible, Is.False, "no change, no warning");

        Named<CheckBox>(rowControl, "editTlsBox").IsChecked = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Multiple(() =>
        {
            Assert.That(warning.IsEffectivelyVisible, Is.True);
            Assert.That(warning.Text, Is.EqualTo("Applying forgets the pinned certificate."));
        });
    }

    [AvaloniaTest]
    public void ApplyingTlsOffOnTheLastTlsRow_DisablesTheVerifySwitch()
    {
        var window = ShowWindow(out SettingsViewModel vm, out _);
        OpenThroughMenu(window, 0);
        Named<CheckBox>(Row(window, 0), "editTlsBox").IsChecked = false;
        Dispatcher.UIThread.RunJobs();

        Named<Button>(Row(window, 0), "applyEditButton").Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        ToggleSwitch toggle = Named<ToggleSwitch>(window, "verifyTlsToggle");
        Assert.Multiple(() =>
        {
            Assert.That(toggle.IsEffectivelyEnabled, Is.False);
            Assert.That(Named<TextBlock>(window, "verifyTlsHint").Text,
                Is.EqualTo("None of this network's servers use TLS, so there is no certificate to check."));
            Assert.That(vm.SelectedNetwork!.VerifyCertificates, Is.True, "the saved value is kept");
        });
    }
}
