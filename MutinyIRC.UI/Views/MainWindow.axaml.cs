using global::Avalonia.Controls;
using global::Avalonia.Interactivity;

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
}
