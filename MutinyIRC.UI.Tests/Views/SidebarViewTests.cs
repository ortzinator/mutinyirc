using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
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

        var server = new Server { Connection = conn };
        server.HookEvents();
        return server;
    }

    /// <summary>Shows the window over a live MainViewModel and lets its bindings settle.</summary>
    private static (Window window, MainViewModel vm) ShowSidebar()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var window = new MainWindow { DataContext = vm };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static TextBlock Heading(Window window, string text) =>
        window.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Classes.Contains("groupHeading") && t.Text == text);

    /// <summary>
    /// The row button a panel is drawn on. A row shares its DataContext with the close button
    /// beside it, so the pill — which only the nav theme has — is what tells them apart.
    /// </summary>
    private static Button RowFor(Window window, object panel) =>
        window.GetVisualDescendants().OfType<Button>()
            .First(b => ReferenceEquals(b.DataContext, panel)
                        && b.GetVisualDescendants().OfType<Border>().Any(x => x.Name == "PART_Pill"));

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
