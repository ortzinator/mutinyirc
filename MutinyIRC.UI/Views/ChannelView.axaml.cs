using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.VisualTree;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

public partial class ChannelView : UserControl
{
    public ChannelView()
    {
        InitializeComponent();
        commandBox.CommandEntered += (_, _) => outputBox.ScrollToBottom();
        sendButton.Click += (_, _) =>
        {
            commandBox.Submit();
            commandBox.Focus();
        };

        // Right-click doesn't select a ListBox row by default, but the user-list context menu
        // targets the selected user — so select the row under the cursor before the menu opens.
        userBox.AddHandler(PointerPressedEvent, UserBox_PointerPressed, RoutingStrategies.Tunnel);
        userBox.SelectionChanged += UserBox_SelectionChanged;
    }

    /// <summary>
    /// The user list holds group headers as well as users, and SelectedItem is typed object, so a
    /// header can be pushed into it. No user input can do that — headers bind IsHitTestVisible and
    /// Focusable to IsSelectable=false, which keeps the pointer off them and makes the ListBox's
    /// own keyboard navigation step straight over them. This is the backstop for a programmatic
    /// set, so the view model's UserViewModel-typed SelectedUser never sees a header.
    /// </summary>
    private void UserBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (userBox.SelectedItem is IUserListRow { IsSelectable: false })
            userBox.SelectedItem = null;
    }

    private void UserBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(userBox).Properties.IsRightButtonPressed)
            return;

        var item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (item != null)
            userBox.SelectedItem = item.DataContext;
    }
}
