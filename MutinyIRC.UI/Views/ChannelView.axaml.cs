namespace MutinyIRC.UI.Views;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.VisualTree;
using UI.Controls;

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
