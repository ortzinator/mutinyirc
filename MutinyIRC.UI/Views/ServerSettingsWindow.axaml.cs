using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Linq;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Views;

public partial class ServerSettingsWindow : Window
{
    public ServerSettingsWindow() : this(new SettingsViewModel()) { }

    /// <summary>
    /// Edits <paramref name="viewModel"/>. The parameterless constructor edits the saved
    /// settings; taking the view model is what lets a test open the window without reading the
    /// user's servers.json.
    /// </summary>
    public ServerSettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        networksListBox.AddHandler(
            LostFocusEvent,
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

    /// <summary>
    /// Renames the network you double-click. The mockup's rail carries no rename affordance,
    /// but a network you have never connected to has no other handle: its name is not yet
    /// coming from the server.
    /// </summary>
    private void NetworksListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var target = ((SettingsViewModel)DataContext!).SelectedNetwork;
        if (target is null || target.IsEditing)
            return;

        target.BeginEdit();
        FocusEditorFor(target);
    }

    /// <summary>Puts the caret in the channel input the moment it appears.</summary>
    private void AddChannelButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => newChannelBox.Focus());
    }

    /// <summary>
    /// Focuses the rename box for <paramref name="target"/>. The box does not exist until the
    /// template reacts to IsEditing, so the search waits a dispatcher turn.
    /// </summary>
    private void FocusEditorFor(NetworkSettingsViewModel target)
    {
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
