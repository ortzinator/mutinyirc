using Avalonia.Controls;
using Avalonia.Interactivity;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
    }

    private Window? Owner => TopLevel.GetTopLevel(this) as Window;

    private async void AppSettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Window? owner = Owner;
        if (owner is null) return;

        await new AppSettingsWindow().ShowDialog(owner);
    }

    // A Flyout stays open when something inside it is clicked, unlike a MenuFlyout, so the
    // switcher must close its own. Hide() detaches the flyout content, which clears the row's
    // DataContext, so read the connection first and show it after the flyout has gone.
    private void NetworkRow_Click(object? sender, RoutedEventArgs e)
    {
        var server = (sender as Control)?.DataContext as ServerViewModel;
        NetworkSwitcherButton.Flyout?.Hide();
        (DataContext as MainViewModel)?.ShowServerCommand.Execute(server);
    }

    // Reached from the switcher's flyout and from the settings menu, so it closes the switcher
    // either way. Hide() on a flyout that is already shut does nothing.
    private async void ManageNetworks_Click(object? sender, RoutedEventArgs e)
    {
        NetworkSwitcherButton.Flyout?.Hide();

        Window? owner = Owner;
        if (owner is null) return;

        await new ServerSettingsWindow().ShowDialog(owner);
    }
}
