using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using System.Linq;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

public partial class ServerSettingsWindow : Window
{
    public ServerSettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();

        networksListBox.AddHandler(
            TextBox.LostFocusEvent,
            (object? _, RoutedEventArgs e) =>
            {
                if (e.Source is TextBox tb && tb.DataContext is NetworkSettingsViewModel netVm && netVm.IsEditing)
                    netVm.CommitEditCommand.Execute(null);
            },
            RoutingStrategies.Bubble);
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

    private void EditNetworkButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = (SettingsViewModel)DataContext!;
        var target = vm.SelectedNetwork;
        if (target is null) return;

        target.BeginEdit();

        Dispatcher.UIThread.Post(() =>
        {
            var textBox = networksListBox.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault(tb => tb.IsVisible && tb.DataContext == target);
            textBox?.Focus();
            textBox?.SelectAll();
        });
    }
}
