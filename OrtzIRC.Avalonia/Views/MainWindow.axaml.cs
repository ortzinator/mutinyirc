namespace OrtzIRC.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Interactivity;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow();
        await dialog.ShowDialog(this);
    }
}
