using System.Linq;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.UI;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Regression coverage for the duplicate-panel bug. After a dropped connection the client
/// reconnects and rejoins its channels; because a <see cref="Channel"/> survives in
/// <c>Server.Channels</c> across the drop, the rejoin re-fires <c>JoinSelf</c> for a channel
/// whose <see cref="ChannelViewModel"/> panel is still open. MainViewModel must reuse that
/// panel rather than open a second one.
/// </summary>
[TestFixture]
public class MainViewModelTests
{
    [SetUp]
    public void Setup()
    {
        // Server_JoinSelf resolves ChannelViewModel instances through the composition root, so
        // the kernel has to be wired before any join is simulated.
        CompositionRoot.Wire(new Bindings());
    }

    // A server backed by a faked (never-connected) Connection. Parsing raw lines into its real
    // Listener drives the same event flow the live socket would, and outgoing commands (e.g. the
    // NAMES request a self-join triggers) are swallowed because the socket is closed.
    private static Server FakeServer(string nick = "me")
    {
        var args = new ConnectionArgs(nick, "irc.fake.com", false);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        return new Server(conn);
    }

    [AvaloniaTest]
    public void RejoiningChannel_DoesNotOpenDuplicatePanel()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var server = FakeServer();
        vm.CreateServerPanel(server);

        // Initial join opens the channel panel.
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        // The reconnect rejoins: CreateChannel hands back the surviving Channel and JoinSelf
        // fires for it a second time.
        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();

        var channelPanels = vm.Panels.OfType<ChannelViewModel>()
            .Where(c => c.Channel == server.Channels["#test"])
            .ToList();
        Assert.That(channelPanels, Has.Count.EqualTo(1),
            "Rejoining a channel must reuse the existing panel, not open a duplicate.");
        Assert.That(vm.SelectedPanel, Is.SameAs(channelPanels[0]),
            "Rejoining should bring the existing channel panel back to the foreground.");
    }

    [AvaloniaTest]
    public void JoiningDistinctChannels_OpensSeparatePanels()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var server = FakeServer();
        vm.CreateServerPanel(server);

        server.Connection.Listener.Parse(":me!u@h JOIN #one");
        server.Connection.Listener.Parse(":me!u@h JOIN #two");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.Panels.OfType<ChannelViewModel>().Count(), Is.EqualTo(2),
            "The dedup guard must not collapse genuinely different channels into one panel.");
    }

    /// <summary>
    /// The channel panel now closes itself off its own server's ChannelRemoved rather than
    /// MainViewModel scanning Panels for it, so this pins that the panel still disappears.
    /// </summary>
    [AvaloniaTest]
    public void PartingChannel_ClosesItsPanel()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        var server = FakeServer();
        vm.CreateServerPanel(server);

        server.Connection.Listener.Parse(":me!u@h JOIN #test");
        Dispatcher.UIThread.RunJobs();
        Assert.That(vm.Panels.OfType<ChannelViewModel>().Count(), Is.EqualTo(1),
            "Precondition: joining opens the panel.");

        server.Connection.Listener.Parse(":me!u@h PART #test");
        Dispatcher.UIThread.RunJobs();

        Assert.That(vm.Panels.OfType<ChannelViewModel>(), Is.Empty,
            "Parting a channel must close its panel.");
    }

    /// <summary>
    /// ServerManager is a singleton, so a MainViewModel that never detaches from ServerAdded
    /// keeps building panels for the rest of the process — the leak that made a second window
    /// impossible.
    /// </summary>
    [AvaloniaTest]
    public void AfterClose_ServerCreatedThroughManager_DoesNotAddPanel()
    {
        var vm = CompositionRoot.Resolve<MainViewModel>();
        vm.Close();
        int panelsAfterClose = vm.Panels.Count;

        Server created = ServerManager.Instance.Create(new ConnectionArgs("me", "irc.fake.com", false));
        try
        {
            Assert.That(vm.Panels.Count, Is.EqualTo(panelsAfterClose),
                "A closed MainViewModel must stop reacting to servers appearing.");
        }
        finally
        {
            ServerManager.Instance.Remove(created);
        }
    }
}
