using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Linq;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.UI.ViewModels.Prototype;

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
        EntryPointEditPrototype.Instance.Attach(viewModel);
        KeyDown += PrototypeSwitch_KeyDown;

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

    // ---- PROTOTYPE, throw away: edit and move of an entry point row (wayfinder ticket #22) ----

    private static EntryPointEditPrototype Proto => EntryPointEditPrototype.Instance;

    private Flyout? _openFlyout;

    private static ServerSettingsViewModel RowOf(object? sender) =>
        (ServerSettingsViewModel)((Control)sender!).DataContext!;

    private void PrototypeSwitch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (FocusManager?.GetFocusedElement() is TextBox)
            return;
        if (e.Key == Key.Left) { Proto.PreviousCommand.Execute(null); e.Handled = true; }
        else if (e.Key == Key.Right) { Proto.NextCommand.Execute(null); e.Handled = true; }
    }

    private void ServerRow_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var row = RowOf(sender);
        if (row.IsEditing)
            return;

        if (Proto.IsA)
            BeginInline(row);
        else if (Proto.IsB)
        {
            var button = ((Control)sender!).GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Flyout is not null);
            button?.Flyout?.ShowAt(button);
        }
        else
            Proto.OpenSheet(row);
    }

    private void BeginInline(ServerSettingsViewModel row)
    {
        row.BeginDraft();
        Dispatcher.UIThread.Post(() =>
        {
            var box = this.GetVisualDescendants().OfType<TextBox>()
                .FirstOrDefault(tb => tb.IsEffectivelyVisible && tb.DataContext == row);
            box?.Focus();
            box?.SelectAll();
        });
    }

    private void MoreButton_Click(object? sender, RoutedEventArgs e)
    {
        var row = RowOf(sender);
        var owner = Proto.OwnerOf(row);
        var menu = new MenuFlyout();

        var edit = new MenuItem { Header = "Edit…" };
        edit.Click += (_, _) => BeginInline(row);
        menu.Items.Add(edit);
        menu.Items.Add(new Separator());

        var move = new MenuItem { Header = "Move to" };
        foreach (var network in Proto.Settings!.Networks.Where(n => n != owner))
        {
            var item = new MenuItem { Header = network.Name };
            item.Click += (_, _) => Proto.MoveTo(row, network);
            move.Items.Add(item);
        }
        move.IsEnabled = move.Items.Count > 0;
        menu.Items.Add(move);

        menu.ShowAt((Control)sender!);
    }

    private void InlineApply_Click(object? sender, RoutedEventArgs e) => Proto.Apply(RowOf(sender));

    private void InlineCancel_Click(object? sender, RoutedEventArgs e) => RowOf(sender).CancelDraft();

    private void EditFlyout_Opening(object? sender, EventArgs e)
    {
        _openFlyout = (Flyout)sender!;
        var row = (ServerSettingsViewModel)_openFlyout.Target!.DataContext!;
        ((Control)_openFlyout.Content!).DataContext = row;
        row.BeginDraft();
    }

    // A light dismiss is a cancel.
    private void EditFlyout_Closed(object? sender, EventArgs e)
    {
        if (((Control)((Flyout)sender!).Content!).DataContext is ServerSettingsViewModel row)
            row.CancelDraft();
        _openFlyout = null;
    }

    private void FlyoutApply_Click(object? sender, RoutedEventArgs e)
    {
        if (Proto.Apply(RowOf(sender)))
            _openFlyout?.Hide();
    }

    private void FlyoutCancel_Click(object? sender, RoutedEventArgs e) => _openFlyout?.Hide();

    private void OpenSheet_Click(object? sender, RoutedEventArgs e) => Proto.OpenSheet(RowOf(sender));

    private void SheetApply_Click(object? sender, RoutedEventArgs e)
    {
        var row = Proto.SheetRow;
        if (row is not null && Proto.Apply(row))
            Proto.CloseSheet();
    }

    private void SheetCancel_Click(object? sender, RoutedEventArgs e)
    {
        Proto.SheetRow?.CancelDraft();
        Proto.CloseSheet();
    }

    // Enter applies and Escape cancels, in every variant. Handled, so Escape does not close the dialog.
    private void Editor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape))
            return;
        e.Handled = true;
        bool apply = e.Key == Key.Enter;
        var row = RowOf(sender);

        if (Proto.IsA)
        {
            if (apply) Proto.Apply(row); else row.CancelDraft();
        }
        else if (Proto.IsB)
        {
            if (apply) FlyoutApply_Click(sender, e); else _openFlyout?.Hide();
        }
        else
        {
            if (apply) SheetApply_Click(sender, e); else SheetCancel_Click(sender, e);
        }
    }
}
