using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class ServerViewModelTests
{
    private Server _server = null!;
    private PluginManager _plugins = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        var args = new ConnectionArgs("me", "irc.fake.com", false);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        _server = new Server(conn);

        _plugins = new PluginManager();
        _vm = new ServerViewModel(_server, _plugins);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
    }

    // A WHOIS reply is an RPL_WHOISUSER (311) followed by RPL_ENDOFWHOIS (318); the server fires
    // WhoisReceived on 318, which the view model prints to the server tab via AddMessage.
    private void ReceiveWhois(string nick = "bob")
    {
        _server.Connection.Listener.Parse($":irc.fake.com 311 me {nick} {nick}user {nick}host * :Real Name");
        _server.Connection.Listener.Parse($":irc.fake.com 318 me {nick} :End of /WHOIS list.");
    }

    [Test]
    public void WhoisReceivedWhileNotSelected_SetsHasUnread()
    {
        Assert.That(_vm.IsSelected, Is.False);
        Assert.That(_vm.HasUnread, Is.False);

        ReceiveWhois();

        Assert.That(_vm.HasUnread, Is.True);
    }

    [Test]
    public void WhoisReceivedWhileSelected_DoesNotSetHasUnread()
    {
        _vm.IsSelected = true;

        ReceiveWhois();

        Assert.That(_vm.HasUnread, Is.False);
    }

    [Test]
    public void SelectingClearsHasUnread()
    {
        ReceiveWhois();
        Assert.That(_vm.HasUnread, Is.True);

        _vm.IsSelected = true;

        Assert.That(_vm.HasUnread, Is.False);
    }

    [Test]
    public void NowAwayReply_SetsIsAway()
    {
        Assert.That(_vm.IsAway, Is.False);

        _server.Connection.Listener.Parse(":irc.fake.com 306 me :You have been marked as being away");

        Assert.That(_vm.IsAway, Is.True);
    }

    [Test]
    public void UnAwayReply_ClearsIsAway()
    {
        _server.Connection.Listener.Parse(":irc.fake.com 306 me :You have been marked as being away");
        Assert.That(_vm.IsAway, Is.True);

        _server.Connection.Listener.Parse(":irc.fake.com 305 me :You are no longer marked as being away");

        Assert.That(_vm.IsAway, Is.False);
    }

    [Test]
    public void ShouldShowAwayReply_FirstTime_ReturnsTrue()
    {
        Assert.That(_vm.ShouldShowAwayReply("bob", "lunch"), Is.True);
    }

    [Test]
    public void ShouldShowAwayReply_SameMessageAgain_ReturnsFalse()
    {
        _vm.ShouldShowAwayReply("bob", "lunch");

        Assert.That(_vm.ShouldShowAwayReply("bob", "lunch"), Is.False,
            "Repeating the same away message for a nick must be suppressed");
    }

    [Test]
    public void ShouldShowAwayReply_ChangedMessage_ReturnsTrue()
    {
        _vm.ShouldShowAwayReply("bob", "lunch");

        Assert.That(_vm.ShouldShowAwayReply("bob", "dinner"), Is.True,
            "A changed away message for a nick must show again");
    }

    [Test]
    public void ShouldShowAwayReply_IsCaseInsensitivePerNick()
    {
        _vm.ShouldShowAwayReply("Bob", "lunch");

        Assert.That(_vm.ShouldShowAwayReply("bob", "lunch"), Is.False,
            "Nick comparison must be case-insensitive");
    }
}
