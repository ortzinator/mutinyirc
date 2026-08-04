using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.VisualTree;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// ChannelView's <c>SelectionChanged</c> handler clears a group header back out of the user list's
/// selection. No user input can put one there — headers are neither hit-testable nor focusable, which
/// <see cref="UserListKeyboardNavigationTests" /> covers — so this handler only ever answers a
/// programmatic set, and nothing in the app makes one today. It stays because of what happens without
/// it: <c>ListBox.SelectedItem</c> is typed <c>object</c> while <c>SelectedUser</c> is typed
/// <c>UserViewModel?</c>, and that binding does not throw or null out on a header, it silently keeps
/// the previous value. The list would then highlight a heading while <c>SelectedUser</c> still pointed
/// at the last real user — and <c>SelectedUser</c> is what the right-click menu's Kick and Ban act on.
/// These tests pin the handler so a later refactor cannot drop it without a failure.
/// </summary>
[TestFixture]
public class UserListSelectionGuardTests
{
    private static (ChannelViewStub stub, ListBox listBox) ShowUserList()
    {
        var stub = new ChannelViewStub(
            new UserViewModel(User.FromNames("@op1")),
            new UserViewModel(User.FromNames("plain1")));

        var view = new ChannelView { DataContext = stub };
        var window = new Window { Content = view, Width = 600, Height = 400 };
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var listBox = view.GetVisualDescendants().OfType<ListBox>().First();
        return (stub, listBox);
    }

    private static void Select(ListBox listBox, object? row)
    {
        listBox.SelectedItem = row;
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static UserGroupHeaderViewModel FirstHeader(ChannelViewStub stub) =>
        stub.UserRows.OfType<UserGroupHeaderViewModel>().First();

    [AvaloniaTest]
    public void SelectingAHeader_LeavesItOutOfTheSelection()
    {
        var (stub, listBox) = ShowUserList();

        Select(listBox, FirstHeader(stub));

        Assert.That(listBox.SelectedItem, Is.Null,
            "A heading pushed into SelectedItem must be cleared straight back out");
    }

    [AvaloniaTest]
    public void SelectingAHeader_DoesNotLeaveSelectedUserPointingAtThePreviousUser()
    {
        var (stub, listBox) = ShowUserList();
        var user = stub.UserList[0];

        Select(listBox, user);
        Assert.That(stub.SelectedUser, Is.SameAs(user), "Baseline: a user row selects normally");

        Select(listBox, FirstHeader(stub));

        Assert.That(stub.SelectedUser, Is.Null,
            $"SelectedUser must not still point at {user.Nick} once the selection has moved off that " +
            "row — the right-click menu would then kick or ban a user the list is no longer showing " +
            "as selected");
    }

    [AvaloniaTest]
    public void SelectingAUserAfterAHeader_StillWorks()
    {
        var (stub, listBox) = ShowUserList();
        var user = stub.UserList[1];

        Select(listBox, FirstHeader(stub));
        Select(listBox, user);

        Assert.That(listBox.SelectedItem, Is.SameAs(user));
        Assert.That(stub.SelectedUser, Is.SameAs(user),
            "Clearing a header must not leave the list unable to select again");
    }
}
