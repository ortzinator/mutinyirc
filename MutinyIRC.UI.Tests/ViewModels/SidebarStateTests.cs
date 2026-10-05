using System.Linq;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the state the sidebar's network switcher and panel list read: which connection the
/// sidebar shows, where switching to a connection lands you, whether a connection you cannot
/// see has unread activity, and how a panel row describes its channel.
/// </summary>
[TestFixture]
public class SidebarStateTests
{
    [SetUp]
    public void Setup()
    {
        // Server_JoinSelf resolves ChannelViewModel instances through the composition root.
        CompositionRoot.Wire(new Bindings());
    }

    private ISender _sender = null!;

    /// <summary>
    /// A connection that reports itself connected and records what it sends, with a real
    /// <see cref="Listener"/> so raw lines drive the same event flow a live socket would.
    /// </summary>
    private Server FakeServer(string host = "irc.fake.com", string nick = "me", bool ssl = false)
    {
        _sender = A.Fake<ISender>();
        var conn = A.Fake<IConnection>();
        A.CallTo(() => conn.Connected).Returns(true);
        A.CallTo(() => conn.Listener).Returns(new Listener());
        A.CallTo(() => conn.Sender).Returns(_sender);
        A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs(nick, host, ssl));

        return new Server(conn);
    }

    // ── Which connection the sidebar shows ──

    [AvaloniaTest]
    public void FirstConnection_BecomesTheShownServerAndItsConsoleIsSelected()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var server = FakeServer();

        vm.CreateServerPanel(server);

        ServerViewModel console = vm.Servers.Single();
        Assert.That(vm.ShownServer, Is.SameAs(console));
        Assert.That(vm.SelectedPanel, Is.SameAs(console),
            "A new connection opens on its console, where its connect and MOTD lines land");
    }

    [AvaloniaTest]
    public void SecondConnection_TakesOverTheSidebar()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        vm.CreateServerPanel(FakeServer("irc.first.com"));
        vm.CreateServerPanel(FakeServer("irc.second.com"));

        Assert.That(vm.ShownServer, Is.SameAs(vm.Servers[1]),
            "A connection you just opened is the one you want to look at");
        Assert.That(vm.SelectedPanel, Is.SameAs(vm.Servers[1]));
    }

    [AvaloniaTest]
    public void JoiningAChannel_ShowsItsConnection()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server first = FakeServer("irc.first.com");
        vm.CreateServerPanel(first);
        vm.CreateServerPanel(FakeServer("irc.second.com"));

        first.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.ShownServer, Is.SameAs(vm.Servers[0]),
            "The sidebar always shows the connection the selected panel belongs to");
    }

    [AvaloniaTest]
    public void SwitchingBack_ReturnsToThePanelThatConnectionWasLastShowing()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server first = FakeServer("irc.first.com");
        vm.CreateServerPanel(first);
        first.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();
        ChannelViewModel channel = vm.Panels.OfType<ChannelViewModel>().Single();

        vm.CreateServerPanel(FakeServer("irc.second.com"));
        vm.ShowServerCommand.Execute(vm.Servers[0]);

        Assert.That(vm.SelectedPanel, Is.SameAs(channel),
            "Switching back to a connection returns you to where you left it");
        Assert.That(vm.ShownServer, Is.SameAs(vm.Servers[0]));
    }

    [AvaloniaTest]
    public void SwitchingToAConnectionNotVisitedYet_SelectsItsConsole()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        vm.CreateServerPanel(FakeServer("irc.first.com"));
        vm.CreateServerPanel(FakeServer("irc.second.com"));

        vm.ShowServerCommand.Execute(vm.Servers[0]);

        Assert.That(vm.SelectedPanel, Is.SameAs(vm.Servers[0]));
    }

    // ── Unread on a connection you cannot see ──

    [AvaloniaTest]
    public void UnreadOnAHiddenConnection_SetsHasUnreadElsewhere()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server first = FakeServer("irc.first.com");
        vm.CreateServerPanel(first);
        first.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        vm.CreateServerPanel(FakeServer("irc.second.com"));
        Assert.That(vm.HasUnreadElsewhere, Is.False, "Precondition: nothing unread yet");

        // A channel message only reaches the panel from someone the channel knows about.
        first.Connection.Listener.Parse(":alice!u@h JOIN #test");
        first.Connection.Listener.Parse(":alice!u@h PRIVMSG #test :ping");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.HasUnreadElsewhere, Is.True,
            "Activity on a connection the sidebar is not showing must mark the switcher");
        Assert.That(vm.Servers[0].HasUnreadAnywhere, Is.True);
    }

    [AvaloniaTest]
    public void UnreadOnTheShownConnection_DoesNotSetHasUnreadElsewhere()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        // Look at the console, so the channel on the same connection collects the unread.
        vm.SelectPanelCommand.Execute(vm.Servers[0]);
        server.Connection.Listener.Parse(":alice!u@h JOIN #test");
        server.Connection.Listener.Parse(":alice!u@h PRIVMSG #test :ping");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.Servers[0].HasUnreadAnywhere, Is.True, "Precondition: the channel is unread");
        Assert.That(vm.HasUnreadElsewhere, Is.False,
            "Unread on the connection you are looking at is already visible in the panel list");
    }

    // ── Closing a channel panel ──

    [AvaloniaTest]
    public void ClosingAJoinedChannelPanel_PartsTheChannel()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        vm.Panels.OfType<ChannelViewModel>().Single().Close();
        Dispatcher.UIThread.RunJobs();

        A.CallTo(() => _sender.Part(A<string>._, "#test")).MustHaveHappened();
    }

    [AvaloniaTest]
    public void ClosingAChannelPanelWeAlreadyLeft_SendsNoPart()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        // The server told us we left, which closes the panel on its own.
        server.Connection.Listener.Parse(":me!u@h PART #test");
        Dispatcher.UIThread.RunJobs();

        A.CallTo(() => _sender.Part(A<string>._, "#test")).MustNotHaveHappened();
    }

    [AvaloniaTest]
    public void ClosingAChannelPanel_SelectsTheConsoleOfTheSameConnection()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        Server server = FakeServer();
        vm.CreateServerPanel(server);
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        vm.Panels.OfType<ChannelViewModel>().Single().Close();
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.SelectedPanel, Is.SameAs(vm.Servers[0]),
            "Closing a panel must leave you on the same connection, not on another one's");
        Assert.That(vm.ShownServer, Is.SameAs(vm.Servers[0]));
    }

    // ── How a row describes its channel ──

    [TestCase("#mutiny", "#", "mutiny")]
    [TestCase("&local", "&", "local")]
    [TestCase("+modeless", "+", "modeless")]
    public void ChannelPanel_SplitsThePrefixFromTheName(string name, string prefix, string bare)
    {
        Server server = FakeServer();
        var channel = new Channel(server, name);
        using var vm = new ChannelViewModel(channel, new PluginManager());

        Assert.That(vm.Prefix, Is.EqualTo(prefix),
            "The row shows the channel's real prefix, so a & channel does not pose as a # one");
        Assert.That(vm.ChannelName, Is.EqualTo(bare));
    }

    /// <summary>
    /// Sets or clears the channel's key the way the server does. The key is not writable from
    /// outside MutinyIRC.Common, so a +k / -k mode change is how a test states one.
    /// </summary>
    private static void SetKey(Channel channel, string? key) =>
        channel.ApplyModeChanges(new[]
        {
            new ChannelModeInfo
            {
                Action = key == null ? ModeAction.Remove : ModeAction.Add,
                Mode = ChannelMode.Password,
                Parameter = key ?? string.Empty
            }
        });

    [Test]
    public void ChannelPanel_FollowsTheChannelKey()
    {
        Server server = FakeServer();
        var channel = new Channel(server, "#mutiny");
        using var vm = new ChannelViewModel(channel, new PluginManager());

        Assert.That(vm.IsKeyed, Is.False, "Precondition: no key yet");

        SetKey(channel, "hunter2");
        Assert.That(vm.IsKeyed, Is.True, "A keyed channel is marked with a padlock");

        SetKey(channel, null);
        Assert.That(vm.IsKeyed, Is.False);
    }

    [Test]
    public void ChannelPanel_OpenedOnAnAlreadyKeyedChannel_IsKeyedFromTheStart()
    {
        Server server = FakeServer();
        var channel = new Channel(server, "#mutiny");
        SetKey(channel, "hunter2");

        using var vm = new ChannelViewModel(channel, new PluginManager());

        Assert.That(vm.IsKeyed, Is.True);
    }
}
