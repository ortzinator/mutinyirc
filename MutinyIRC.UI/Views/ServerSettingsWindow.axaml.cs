using Avalonia;
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

    /// <summary>Opens the editor of the row that the Edit… item of the row menu belongs to.</summary>
    private void EditMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: ServerSettingsViewModel row })
            OpenEntryPointEditor(row);
    }

    /// <summary>
    /// Opens the editor of the row you double-click. A double-click on a button of the row is
    /// two clicks on that button, so it does not open the editor.
    /// </summary>
    private void ServerRow_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Visual source && source.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;
        if (sender is not Control { DataContext: ServerSettingsViewModel row } || row.IsEditing)
            return;

        OpenEntryPointEditor(row);
    }

    /// <summary>
    /// Opens the editor of <paramref name="row"/> and puts the caret in its host field, with the
    /// host selected. The field does not exist until the template reacts to the editor, so the
    /// search waits a dispatcher turn.
    /// </summary>
    private void OpenEntryPointEditor(ServerSettingsViewModel row)
    {
        NetworkSettingsViewModel? network = ((SettingsViewModel)DataContext!).SelectedNetwork;
        if (network is null)
            return;

        network.EditServerCommand.Execute(row);
        Dispatcher.UIThread.Post(() =>
        {
            TextBox? host = RowControlFor(row)?.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault(tb => tb.Name == "editHostBox");
            host?.Focus();
            host?.SelectAll();
        });
    }

    /// <summary>
    /// Enter applies and Escape cancels. The handler marks both keys as handled, also when Apply
    /// is disabled, so that Enter never saves the dialog and Escape never closes it.
    /// </summary>
    private void EntryPointEditor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape))
            return;
        if (sender is not Control { DataContext: EntryPointEditorViewModel editor } control)
            return;

        e.Handled = true;
        ServerSettingsViewModel? row = RowOf(control);
        if (e.Key == Key.Escape)
            editor.CancelCommand.Execute(null);
        else if (editor.ApplyCommand.CanExecute(null))
            editor.ApplyCommand.Execute(null);
        else
            return;

        FocusRowMenuButton(row);
    }

    /// <summary>Gives the focus back to the row after a click on Apply or Cancel.</summary>
    private void EditorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control)
            FocusRowMenuButton(RowOf(control));
    }

    // Gets the entry point row that a control of its editor is in.
    private static ServerSettingsViewModel? RowOf(Control control) =>
        control.GetVisualAncestors()
            .OfType<Border>()
            .FirstOrDefault(b => b.Classes.Contains("serverRow"))
            ?.DataContext as ServerSettingsViewModel;

    // Gets the drawn row of an entry point, or null when the dialog does not show it.
    private Border? RowControlFor(ServerSettingsViewModel row) =>
        this.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Classes.Contains("serverRow") && b.DataContext == row);

    /// <summary>
    /// Puts the focus on the menu button of <paramref name="row"/>. The editor closes before
    /// this runs, and the focus must not stay on a field that is gone. The search waits a
    /// dispatcher turn, because the row draws its values again only after that.
    /// </summary>
    private void FocusRowMenuButton(ServerSettingsViewModel? row)
    {
        if (row is null)
            return;

        Dispatcher.UIThread.Post(() =>
            RowControlFor(row)?.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Name == "rowMenuButton")
                ?.Focus());
    }
}
