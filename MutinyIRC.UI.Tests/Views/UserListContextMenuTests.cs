using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Verifies the user-list right-click context menu in ChannelView. The menu lives on the ListBox and
/// targets the selected user, so its commands must resolve against the channel view model and its
/// CommandParameter must carry the selected <see cref="UserViewModel"/>. These tests guard that
/// wiring, which is easy to break with a stray binding-path or namescope change.
/// </summary>
[TestFixture]
public class UserListContextMenuTests
{
    /// <summary>
    /// Minimal stand-in for ChannelViewModel exposing only what ChannelView's user list binds to.
    /// We avoid the real ChannelViewModel because it needs a live Channel/Server/PluginManager.
    /// Each command records the user it was invoked with so the test can assert the parameter.
    /// </summary>
    private sealed class ChannelStub
    {
        public List<UserViewModel> UserList { get; }
        public UserViewModel? LastInvokedUser { get; private set; }

        public ICommand WhoisUserCommand { get; }
        public ICommand QueryUserCommand { get; }
        public ICommand OpUserCommand { get; }
        public ICommand DeopUserCommand { get; }
        public ICommand VoiceUserCommand { get; }
        public ICommand DevoiceUserCommand { get; }
        public ICommand KickUserCommand { get; }
        public ICommand BanUserCommand { get; }
        public ICommand KickBanUserCommand { get; }

        public ChannelStub(params UserViewModel[] users)
        {
            UserList = users.ToList();
            ICommand Record() => new RelayCommand<UserViewModel>(u => LastInvokedUser = u);
            WhoisUserCommand = Record();
            QueryUserCommand = Record();
            OpUserCommand = Record();
            DeopUserCommand = Record();
            VoiceUserCommand = Record();
            DevoiceUserCommand = Record();
            KickUserCommand = Record();
            BanUserCommand = Record();
            KickBanUserCommand = Record();
        }
    }

    private static (ChannelStub stub, ContextMenu menu, UserViewModel user) OpenMenuForSelectedUser()
    {
        var user = new UserViewModel(new User("alice", "ident", "host"));
        var stub = new ChannelStub(user);

        var view = new ChannelView { DataContext = stub };
        var window = new Window { Content = view, Width = 600, Height = 400 };
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var listBox = view.GetVisualDescendants().OfType<ListBox>().First();
        listBox.SelectedItem = user;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var menu = listBox.ContextMenu;
        Assert.That(menu, Is.Not.Null, "User list should carry a ContextMenu");

        menu!.Open(listBox);
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return (stub, menu, user);
    }

    private static MenuItem ItemNamed(ContextMenu menu, string header) =>
        menu.GetLogicalDescendants().OfType<MenuItem>().First(m => (string?)m.Header == header);

    [AvaloniaTest]
    public void Whois_RoutesSelectedUserToChannelViewModel()
    {
        var (stub, menu, user) = OpenMenuForSelectedUser();

        var whois = ItemNamed(menu, "Whois");
        Assert.That(whois.Command, Is.Not.Null,
            "Command binding must resolve against the channel view model");
        Assert.That(whois.CommandParameter, Is.SameAs(user),
            "CommandParameter must be the selected user");

        whois.Command!.Execute(whois.CommandParameter);
        Assert.That(stub.LastInvokedUser, Is.SameAs(user));
    }

    [AvaloniaTest]
    [TestCase("Private message")]
    [TestCase("Op")]
    [TestCase("Deop")]
    [TestCase("Voice")]
    [TestCase("Devoice")]
    [TestCase("Kick")]
    [TestCase("Ban")]
    [TestCase("Kick + Ban")]
    public void MenuItem_ResolvesCommandAndSelectedUser(string header)
    {
        var (stub, menu, user) = OpenMenuForSelectedUser();

        var item = ItemNamed(menu, header);
        Assert.That(item.Command, Is.Not.Null, $"'{header}' must resolve its command binding");
        Assert.That(item.CommandParameter, Is.SameAs(user), $"'{header}' must carry the selected user");

        item.Command!.Execute(item.CommandParameter);
        Assert.That(stub.LastInvokedUser, Is.SameAs(user), $"'{header}' must target the selected user");
    }
}
