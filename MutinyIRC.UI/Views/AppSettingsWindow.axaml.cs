using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

public partial class AppSettingsWindow : Window
{
    public AppSettingsWindow()
    {
        InitializeComponent();
        DataContext = new AppSettingsViewModel();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ((AppSettingsViewModel)DataContext!).Save();
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
