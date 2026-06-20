using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Verifies the user-list right-click context menu in ChannelView. Every menu item binds to one
/// <c>UserCommand</c> and passes its IRC verb as the parameter, while the target user comes from the
/// ListBox's two-way <c>SelectedItem</c> binding. These tests guard both halves: that selecting a row
/// populates <c>SelectedUser</c>, and that each item carries the exact verb it is supposed to run —
/// the strings most likely to break with a typo (e.g. <c>/mode +o</c>, <c>/ban -k</c>).
/// </summary>
[TestFixture]
public class UserListContextMenuTests
{
    /// <summary>
    /// Header text paired with the IRC verb the menu item must pass as its CommandParameter.
    /// Mirrors the bindings in ChannelView.axaml so a renamed header or altered verb fails here.
    /// </summary>
    private static readonly (string Header, string Verb)[] MenuVerbs =
    {
        ("Whois", "/whois"),
        ("Private message", "/query"),
        ("Op", "/mode +o"),
        ("Deop", "/mode -o"),
        ("Voice", "/mode +v"),
        ("Devoice", "/mode -v"),
        ("Kick", "/kick"),
        ("Ban", "/ban"),
        ("Kick + Ban", "/ban -k"),
    };

    /// <summary>
    /// Minimal stand-in for ChannelViewModel exposing only what ChannelView's user list binds to.
    /// We avoid the real ChannelViewModel because it needs a live Channel/Server/PluginManager.
    /// <c>UserCommand</c> records the verb it was invoked with so the test can assert it.
    /// </summary>
    private sealed class ChannelStub : ObservableObject
    {
        public List<UserViewModel> UserList { get; }

        private UserViewModel? _selectedUser;
        public UserViewModel? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string? LastInvokedVerb { get; private set; }
        public ICommand UserCommand { get; }

        public ChannelStub(params UserViewModel[] users)
        {
            UserList = users.ToList();
            UserCommand = new RelayCommand<string>(verb => LastInvokedVerb = verb);
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
    public void SelectingRow_PopulatesSelectedUser()
    {
        var (stub, _, user) = OpenMenuForSelectedUser();

        Assert.That(stub.SelectedUser, Is.SameAs(user),
            "ListBox SelectedItem must flow into the view model's SelectedUser");
    }

    [AvaloniaTest]
    public void EveryMenuItem_BindsUserCommandAndCarriesItsVerb(
        [ValueSource(nameof(MenuVerbs))] (string Header, string Verb) entry)
    {
        var (stub, menu, _) = OpenMenuForSelectedUser();

        var item = ItemNamed(menu, entry.Header);
        Assert.That(item.Command, Is.Not.Null, $"'{entry.Header}' must resolve its UserCommand binding");
        Assert.That(item.CommandParameter, Is.EqualTo(entry.Verb),
            $"'{entry.Header}' must pass the verb '{entry.Verb}'");

        item.Command!.Execute(item.CommandParameter);
        Assert.That(stub.LastInvokedVerb, Is.EqualTo(entry.Verb),
            $"'{entry.Header}' must run the verb '{entry.Verb}'");
    }
}
