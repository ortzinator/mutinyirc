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
}
