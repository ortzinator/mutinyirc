using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MutinyIRC.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void AppSettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new AppSettingsWindow();
        await dialog.ShowDialog(this);
    }

    private async void ServersMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new ServerSettingsWindow();
        await dialog.ShowDialog(this);
    }

    // A Flyout stays open when something inside it is clicked, unlike a MenuFlyout, so the
    // switcher closes its own after the row has run its command.
    private void NetworkRow_Click(object? sender, RoutedEventArgs e)
    {
        NetworkSwitcherButton.Flyout?.Hide();
    }

    private async void ManageNetworks_Click(object? sender, RoutedEventArgs e)
    {
        NetworkSwitcherButton.Flyout?.Hide();
        var dialog = new ServerSettingsWindow();
        await dialog.ShowDialog(this);
    }
}
