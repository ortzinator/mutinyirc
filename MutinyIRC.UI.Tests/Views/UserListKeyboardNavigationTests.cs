using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.Views;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// The user list is one flat ListBox holding group headers as well as users, so keyboard
/// navigation could walk onto a heading. It doesn't: headers report
/// <see cref="IUserListRow.IsSelectable" /> false, ChannelView binds <c>Focusable</c> to that, and
/// Avalonia's own selection movement steps over anything non-focusable. That behaviour is
/// load-bearing — <c>SelectedUser</c> is typed <c>UserViewModel?</c> and cannot hold a header — but
/// it is entirely implicit in a XAML setter, so these tests pin it.
/// </summary>
[TestFixture]
public class UserListKeyboardNavigationTests
{
    // Rows: OPERATORS | @op1 | @op2 | MEMBERS | plain1 | plain2 — two groups, so a header sits
    // both at the top of the list and in the middle of it.
    private const int Op1Index = 1;

    private static (ChannelViewStub stub, ListBox listBox, Window window) ShowUserList()
    {
        var stub = new ChannelViewStub(
            new UserViewModel(User.FromNames("@op1")),
            new UserViewModel(User.FromNames("@op2")),
            new UserViewModel(User.FromNames("plain1")),
            new UserViewModel(User.FromNames("plain2")));

        var view = new ChannelView { DataContext = stub };
        var window = new Window { Content = view, Width = 600, Height = 400 };
        window.Show();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var listBox = view.GetVisualDescendants().OfType<ListBox>().First();
        return (stub, listBox, window);
    }

    /// <summary>
    /// Focus a specific container rather than the ListBox: the message CommandTextBox takes focus
    /// when the view is shown, so keys sent without this land in the input box and every assertion
    /// below passes vacuously.
    /// </summary>
    private static void FocusRow(ListBox listBox, int index)
    {
        var container = listBox.ContainerFromIndex(index) as ListBoxItem;
        Assert.That(container, Is.Not.Null, $"Row {index} should have a realised container");
        container!.Focus();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static void Press(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(key, RawInputModifiers.None);
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void ArrowDown_StepsOverTheGroupHeader()
    {
        var (stub, listBox, window) = ShowUserList();

        var selections = new List<object?>();
        listBox.SelectionChanged += (_, _) => selections.Add(listBox.SelectedItem);

        FocusRow(listBox, Op1Index);
        Press(window, PhysicalKey.ArrowDown);
        Press(window, PhysicalKey.ArrowDown);

        Assert.That((listBox.SelectedItem as UserViewModel)?.Nick, Is.EqualTo("plain1"),
            "Arrow-down from @op2 must land on plain1, not on the MEMBERS heading between them");
        Assert.That(selections, Has.None.InstanceOf<UserGroupHeaderViewModel>(),
            "A heading must never become the selection, even in passing");
        Assert.That(stub.SelectedUser?.Nick, Is.EqualTo("plain1"),
            "The walk must leave SelectedUser populated, not nulled by the header guard");
    }

    [AvaloniaTest]
    public void Home_SkipsTheHeadingAtTheTopOfTheList()
    {
        var (stub, listBox, window) = ShowUserList();

        FocusRow(listBox, Op1Index);
        Press(window, PhysicalKey.Home);

        Assert.That((listBox.SelectedItem as UserViewModel)?.Nick, Is.EqualTo("op1"),
            "Home must reach the first user, not the OPERATORS heading that precedes it");
        Assert.That(stub.SelectedUser?.Nick, Is.EqualTo("op1"));
    }

    [AvaloniaTest]
    public void Headers_AreNeitherFocusableNorHitTestable()
    {
        var (_, listBox, _) = ShowUserList();

        var header = listBox.ContainerFromIndex(0) as ListBoxItem;
        var user = listBox.ContainerFromIndex(Op1Index) as ListBoxItem;

        Assert.That(header!.DataContext, Is.InstanceOf<UserGroupHeaderViewModel>());
        Assert.That(header.Focusable, Is.False, "ChannelView binds ListBoxItem.Focusable to IsSelectable");
        Assert.That(header.IsHitTestVisible, Is.False, "ChannelView binds IsHitTestVisible to IsSelectable");
        Assert.That(user!.Focusable, Is.True);
        Assert.That(user.IsHitTestVisible, Is.True);
    }
}
