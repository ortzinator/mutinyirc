using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class ChannelViewModelTests
{
    private Server _server = null!;
    private PluginManager _plugins = null!;
    private Channel _channel = null!;
    private User _alice = null!;
    private ChannelViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        var args = new ConnectionArgs("me", "irc.fake.com", false);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        _server = new Server(conn);

        _plugins = new PluginManager();
        _channel = new Channel(_server, "#mutiny");

        // OnNewMessage/OnNewAction only fire for a sender already in the user list.
        _alice = new User { Nick = "alice" };
        _channel.Users.Add(_alice);

        _vm = new ChannelViewModel(_channel, _plugins);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
    }

    [Test]
    public void ReceivingMessageWhileNotSelected_SetsHasUnread()
    {
        Assert.That(_vm.IsSelected, Is.False);
        Assert.That(_vm.HasUnread, Is.False);

        _channel.OnNewMessage(_alice, "ping");

        Assert.That(_vm.HasUnread, Is.True);
    }

    [Test]
    public void ReceivingActionWhileNotSelected_SetsHasUnread()
    {
        _channel.OnNewAction(_alice, "waves");

        Assert.That(_vm.HasUnread, Is.True);
    }

    [Test]
    public void ReceivingMessageWhileSelected_DoesNotSetHasUnread()
    {
        _vm.IsSelected = true;

        _channel.OnNewMessage(_alice, "ping");

        Assert.That(_vm.HasUnread, Is.False);
    }

    [Test]
    public void SelectingClearsHasUnread()
    {
        _channel.OnNewMessage(_alice, "ping");
        Assert.That(_vm.HasUnread, Is.True);

        _vm.IsSelected = true;

        Assert.That(_vm.HasUnread, Is.False);
    }

    // --- Panel closes itself when its channel stops being tracked ---
    //
    // Server.ChannelRemoved is per-connection, so the panel watches its own server and decides
    // for itself whether the removal was about it. Parsing a self-PART drives the real path:
    // Listener_OnPart drops the channel from Server.Channels and fires ChannelRemoved.

    [Test]
    public void ChannelRemoved_ForThisChannel_RequestsClose()
    {
        _server.Channels.Add(_channel.Name, _channel);
        bool closeRequested = false;
        _vm.RequestClose += (_, _) => closeRequested = true;

        _server.Connection.Listener.Parse(":me!u@h PART #mutiny");

        Assert.That(closeRequested, Is.True,
            "A self-part must close the panel for the channel that went away.");
    }

    [Test]
    public void ChannelRemoved_ForAnotherChannel_DoesNotRequestClose()
    {
        _server.Channels.Add(_channel.Name, _channel);
        _server.Channels.Add("#other", new Channel(_server, "#other"));
        bool closeRequested = false;
        _vm.RequestClose += (_, _) => closeRequested = true;

        _server.Connection.Listener.Parse(":me!u@h PART #other");

        Assert.That(closeRequested, Is.False,
            "Parting a different channel on the same connection must leave this panel open.");
    }

    [Test]
    public void AfterDispose_ChannelRemoved_DoesNotRequestClose()
    {
        _server.Channels.Add(_channel.Name, _channel);
        bool closeRequested = false;
        _vm.RequestClose += (_, _) => closeRequested = true;

        _vm.Dispose();
        _server.Connection.Listener.Parse(":me!u@h PART #mutiny");

        Assert.That(closeRequested, Is.False,
            "Dispose must detach the subscription so a disposed panel stops reacting.");
    }
}
