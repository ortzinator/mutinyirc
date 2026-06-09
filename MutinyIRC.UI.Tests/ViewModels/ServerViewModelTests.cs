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
