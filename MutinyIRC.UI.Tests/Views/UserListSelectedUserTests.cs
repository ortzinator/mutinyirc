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
/// The user list binds <c>SelectedItem</c> to <c>SelectedRow</c>, typed <see cref="IUserListRow" />
/// so it can carry a heading as well as a user. These tests cover only what needs a real view: that
/// the binding round-trips both kinds without the silent write failure a <c>UserViewModel</c>-typed
/// target would give on a heading. What <c>SelectedUser</c> then reports is production logic on
/// <c>ChannelViewModel</c> and is pinned in <c>ChannelViewModelTests</c>, not here — this view's
/// DataContext is a stub, so an assertion about the projection would only be testing the stub.
/// </summary>
[TestFixture]
public class UserListSelectedUserTests
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

    [AvaloniaTest]
    public void SelectingAUser_FlowsIntoSelectedRow()
    {
        var (stub, listBox) = ShowUserList();
        var user = stub.UserList[0];

        Select(listBox, user);

        Assert.That(stub.SelectedRow, Is.SameAs(user),
            "ListBox SelectedItem must flow into the view model's SelectedRow");
    }

    [AvaloniaTest]
    public void SelectingAHeading_FlowsIntoSelectedRowRatherThanFailingSilently()
    {
        var (stub, listBox) = ShowUserList();
        var heading = stub.UserRows.OfType<UserGroupHeaderViewModel>().First();

        Select(listBox, stub.UserList[0]);
        Select(listBox, heading);

        Assert.That(stub.SelectedRow, Is.SameAs(heading),
            "A heading must reach SelectedRow. A UserViewModel-typed binding target would drop this " +
            "write without complaint and strand the previous user there.");
    }
}
