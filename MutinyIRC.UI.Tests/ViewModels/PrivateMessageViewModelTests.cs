using System.Linq;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class PrivateMessageViewModelTests
{
    private Server _server = null!;
    private PluginManager _plugins = null!;
    private PrivateMessageSession _session = null!;
    private PrivateMessageViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        var args = new ConnectionArgs("me", "irc.fake.com", false);
        var conn = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        _server = new Server(conn);

        _plugins = new PluginManager();
        _session = _server.GetOrCreatePM(new User { Nick = "alice" });
        _vm = new PrivateMessageViewModel(_session, _plugins);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
    }

    [Test]
    public void Ctor_SetsOtherNickAndName()
    {
        Assert.That(_vm.OtherNick, Is.EqualTo("alice"));
        Assert.That(_vm.Name, Is.EqualTo("alice"));
    }

    [Test]
    public void MessageReceived_AddsChannelMessageViewModel()
    {
        _session.OnMessageReceived(new Common.DataEventArgs<string>("hi"));

        var last = _vm.ChatLines.Last() as ChannelMessageViewModel;
        Assert.That(last, Is.Not.Null);
        Assert.That(last!.Message, Is.EqualTo("hi"));
        Assert.That(last.User.Nick, Is.EqualTo("alice"));
    }

    [Test]
    public void ActionReceived_AddsChannelActionViewModel()
    {
        _session.OnActionReceived(new Common.DataEventArgs<string>("waves"));

        var last = _vm.ChatLines.Last() as ChannelActionViewModel;
        Assert.That(last, Is.Not.Null);
        Assert.That(last!.Message, Is.EqualTo("waves"));
    }

    [Test]
    public void MessageSent_AddsLineWithOwnNick()
    {
        _session.Send("hello back");

        var last = _vm.ChatLines.Last() as ChannelMessageViewModel;
        Assert.That(last, Is.Not.Null);
        Assert.That(last!.Message, Is.EqualTo("hello back"));
        Assert.That(last.User.Nick, Is.EqualTo("me"));
    }

    [Test]
    public void ReceivingWhileNotSelected_SetsHasUnread()
    {
        Assert.That(_vm.IsSelected, Is.False);
        Assert.That(_vm.HasUnread, Is.False);

        _session.OnMessageReceived(new Common.DataEventArgs<string>("ping"));

        Assert.That(_vm.HasUnread, Is.True);
    }

    [Test]
    public void ReceivingWhileSelected_DoesNotSetHasUnread()
    {
        _vm.IsSelected = true;

        _session.OnMessageReceived(new Common.DataEventArgs<string>("ping"));

        Assert.That(_vm.HasUnread, Is.False);
    }

    [Test]
    public void SelectingClearsHasUnread()
    {
        _session.OnMessageReceived(new Common.DataEventArgs<string>("ping"));
        Assert.That(_vm.HasUnread, Is.True);

        _vm.IsSelected = true;

        Assert.That(_vm.HasUnread, Is.False);
    }

    [Test]
    public void Close_FiresRequestClose()
    {
        // Closure is UI-driven: the VM only signals RequestClose; the host
        // (MainViewModel) is responsible for removing the session and disposing.
        bool requestedClose = false;
        _vm.RequestClose += (_, _) => requestedClose = true;

        _vm.Close();

        Assert.That(requestedClose, Is.True);
    }
}
