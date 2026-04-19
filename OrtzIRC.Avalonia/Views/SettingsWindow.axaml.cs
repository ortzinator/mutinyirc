namespace OrtzIRC.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using OrtzIRC.Avalonia.ViewModels;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ((SettingsViewModel)DataContext!).Save();
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
