using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Drives the sidebar through a real <see cref="MainWindow"/>, so every binding it reads has to
/// resolve. These are the checks a screenshot cannot make: whether a heading is hidden when its
/// group is empty, whether the selection pill is on the row that is selected, and whether the
/// padlock follows the channel's key.
/// </summary>
[TestFixture]
public class SidebarViewTests
{
    [SetUp]
    public void Setup()
    {
        // A self-join builds its channel panel through the kernel.
        CompositionRoot.Wire(new Bindings());
    }

    private static Server FakeServer(string host = "irc.fake.com")
    {
        var conn = A.Fake<IConnection>();
        A.CallTo(() => conn.Connected).Returns(true);
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.Sender).Returns(A.Fake<ISender>());
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("me", host, false));

        return new Server(conn);
    }

    /// <summary>
    /// Shows a bare <see cref="SidebarView"/> over a live MainViewModel and lets its bindings
    /// settle. The sidebar is hosted on its own rather than inside a MainWindow: it reaches no
    /// further than itself, and nothing the content area draws can be mistaken for part of it.
    /// </summary>
    private static (Window window, MainViewModel vm) ShowSidebar()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var window = new Window { Content = new SidebarView(), DataContext = vm };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static TextBlock Heading(Window window, string text) =>
        window.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Classes.Contains("groupHeading") && t.Text == text);

    /// <summary>
    /// The row button a panel is drawn on. The close button inside the row inherits the row's
    /// DataContext, so the row is the one of the two that no template owns.
    /// </summary>
    private static Button RowFor(Window window, object panel) =>
        window.GetVisualDescendants().OfType<Button>()
            .Single(b => ReferenceEquals(b.DataContext, panel) && b.TemplatedParent is null);

    /// <summary>The close button folded into a row's template.</summary>
    private static Button CloseButtonOf(Window window, object panel) =>
        RowFor(window, panel).GetVisualDescendants().OfType<Button>()
            .Single(b => b.Name == "PART_Close");

    [AvaloniaTest]
    public void Switcher_NamesTheShownConnectionAndItsState()
    {
        var (window, vm) = ShowSidebar();
        vm.CreateServerPanel(FakeServer("irc.libera.chat"));
        Dispatcher.UIThread.RunJobs();

        var texts = window.GetVisualDescendants().OfType<TextBlock>()
            .Select(t => t.Text).ToList();

        Assert.That(texts, Does.Contain("irc.libera.chat"),
            "Before it registers, the host is the only name the switcher has for the network");
        Assert.That(texts, Does.Contain("Connected"),
            "The switcher's second line is the connection's state");
    }

    [AvaloniaTest]
    public void SwitcherRow_ShowsThatConnectionAndClosesTheFlyout()
    {
        var (window, vm) = ShowSidebar();
        vm.CreateServerPanel(FakeServer("irc.a.com"));
        vm.CreateServerPanel(FakeServer("irc.b.com"));
        ServerViewModel first = vm.Servers.First();
        vm.ShowServerCommand.Execute(vm.Servers.Last());
        Dispatcher.UIThread.RunJobs();

        Button switcher = window.GetVisualDescendants().OfType<Button>()
            .Single(b => b.Name == "NetworkSwitcherButton");
        var flyout = (Flyout)switcher.Flyout!;
        flyout.ShowAt(switcher);
        Dispatcher.UIThread.RunJobs();

        Button row = ((Control)flyout.Content!).GetVisualDescendants().OfType<Button>()
            .First(b => ReferenceEquals(b.DataContext, first));
        row.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.ShownServer, Is.SameAs(first),
            "Closing the flyout clears the row's DataContext, so the click has to read the "
            + "connection before it hides");
        Assert.That(flyout.IsOpen, Is.False, "The row that has been chosen closes the switcher");
    }

    [AvaloniaTest]
    public void PanelHeadings_AreAbsentWhileThereIsNoConnection()
    {
        var (window, _) = ShowSidebar();

        var shown = window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.Classes.Contains("groupHeading") && t.IsEffectivelyVisible)
            .Select(t => t.Text).ToList();

        Assert.That(shown, Is.Empty,
            "With no connection there are no panels, so a heading has nothing to head");
    }

    [AvaloniaTest]
    public void ChannelsHeading_IsHiddenUntilTheConnectionHasAChannel()
    {
        var (window, vm) = ShowSidebar();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        Dispatcher.UIThread.RunJobs();

        Assert.That(Heading(window, "CHANNELS").IsVisible, Is.False,
            "A heading over an empty group is a label for nothing");

        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        Assert.That(Heading(window, "CHANNELS").IsVisible, Is.True);
        Assert.That(Heading(window, "DIRECT MESSAGES").IsVisible, Is.False,
            "Joining a channel must not raise the direct-message heading");
    }

    [AvaloniaTest]
    public void ChannelRow_DrawsThePrefixItselfRatherThanAssumingAHash()
    {
        var (window, vm) = ShowSidebar();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN &local");
        Dispatcher.UIThread.RunJobs();

        ChannelViewModel channel = vm.Panels.OfType<ChannelViewModel>().Single();
        Button row = RowFor(window, channel);
        var texts = row.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();

        Assert.That(texts, Does.Contain("&"), "A & channel must not be drawn as a # one");
        Assert.That(texts, Does.Contain("local"));
    }

    [AvaloniaTest]
    public void SelectedRow_IsTheOneCarryingTheAccentPill()
    {
        var (window, vm) = ShowSidebar();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        ChannelViewModel channel = vm.Panels.OfType<ChannelViewModel>().Single();
        ServerViewModel console = vm.Servers.Single();

        // Joining selects the channel, so its row has the pill and the console's does not.
        Assert.That(PillOf(window, channel).IsVisible, Is.True);
        Assert.That(PillOf(window, console).IsVisible, Is.False);

        vm.SelectPanelCommand.Execute(console);
        Dispatcher.UIThread.RunJobs();

        Assert.That(PillOf(window, console).IsVisible, Is.True,
            "The pill must follow the selection");
        Assert.That(PillOf(window, channel).IsVisible, Is.False);
    }

    private static Border PillOf(Window window, object panel) =>
        RowFor(window, panel).GetVisualDescendants().OfType<Border>()
            .First(b => b.Name == "PART_Pill");

    [AvaloniaTest]
    public void CloseButton_BelongsToTheRowThePointerIsOnAndSaysWhatClosingCosts()
    {
        var (window, vm) = ShowSidebar();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        server.Connection.Listener.Parse(":bob!u@h PRIVMSG me :hello");
        Dispatcher.UIThread.RunJobs();

        ChannelViewModel channel = vm.Panels.OfType<ChannelViewModel>().Single();
        PrivateMessageViewModel dm = vm.Panels.OfType<PrivateMessageViewModel>().Single();
        ServerViewModel console = vm.Servers.Single();

        Button close = CloseButtonOf(window, channel);
        Assert.That(close.IsEffectivelyVisible, Is.False,
            "A row nobody points at shows no close button, and so cannot be closed by a stray "
            + "click on the space one would occupy");

        Hover(window, RowFor(window, channel));
        Assert.That(close.IsEffectivelyVisible, Is.True);
        Assert.That(close.Command, Is.SameAs(channel.CloseCommand),
            "The close button is part of the row, so it closes that row's panel");
        Assert.That(ToolTip.GetTip(close), Is.EqualTo("Leave and close"),
            "Closing a channel panel parts the channel, and the row says so");

        Assert.That(ToolTip.GetTip(CloseButtonOf(window, dm)), Is.EqualTo("Close"),
            "Closing a direct message costs nothing, so it does not warn");

        Hover(window, RowFor(window, console));
        Assert.That(CloseButtonOf(window, console).IsEffectivelyVisible, Is.False,
            "The console is the connection itself: it closes with the connection, not on its own");
    }

    /// <summary>Puts the pointer on the middle of a control.</summary>
    private static void Hover(Window window, Control control)
    {
        Point centre = control
            .TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!
            .Value;
        window.MouseMove(centre);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void ChannelRow_ShowsThePadlockOnlyWhileTheChannelIsKeyed()
    {
        var (window, vm) = ShowSidebar();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        ChannelViewModel channel = vm.Panels.OfType<ChannelViewModel>().Single();
        Button row = RowFor(window, channel);
        FluentIcons.Avalonia.SymbolIcon padlock = row.GetVisualDescendants()
            .OfType<FluentIcons.Avalonia.SymbolIcon>()
            .First(i => i.Symbol == FluentIcons.Common.Symbol.LockClosed);

        Assert.That(padlock.IsVisible, Is.False, "An open channel carries no padlock");

        server.Connection.Listener.Parse(":bob!u@h MODE #test +k hunter2");
        Dispatcher.UIThread.RunJobs();

        Assert.That(padlock.IsVisible, Is.True,
            "A channel that gains a key must gain the padlock without a rejoin");
    }
}
