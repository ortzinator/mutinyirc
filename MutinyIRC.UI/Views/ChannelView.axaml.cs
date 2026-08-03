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
    /// The user list holds group headers as well as users. Headers aren't hit-testable, so the
    /// pointer can't land on one, but arrow keys still walk over them — drop the selection rather
    /// than push a header into the view model's UserViewModel-typed SelectedUser.
    /// </summary>
    private void UserBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (userBox.SelectedItem is not null and not UserViewModel)
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
