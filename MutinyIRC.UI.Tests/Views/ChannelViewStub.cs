using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.Views;

/// <summary>
/// Minimal stand-in for ChannelViewModel exposing only what ChannelView's user list binds to.
/// We avoid the real ChannelViewModel because it needs a live Channel/Server/PluginManager.
/// Shared by every user-list view test so the binding surface is declared once — XAML binding is
/// duck-typed, so a second copy could drift from the real view model without failing to compile.
/// <c>UserCommand</c> records the verb it was invoked with so a test can assert it.
/// </summary>
internal sealed class ChannelViewStub : ObservableObject
{
    public List<UserViewModel> UserList { get; }

    /// <summary>What the ListBox actually binds to: group headers interleaved with users.</summary>
    public IReadOnlyList<IUserListRow> UserRows { get; }

    public string UserFilter { get; set; } = string.Empty;
    public string UserFilterWatermark => $"Filter {UserList.Count} members";

    /// <summary>
    /// What the ListBox's SelectedItem binds to. Deliberately the only selection member here: the
    /// real view model also projects a <c>SelectedUser</c> off this, but that projection is
    /// production logic and is tested against <c>ChannelViewModel</c> itself. Copying it here would
    /// let a view test assert green against the copy while the real one was broken.
    /// </summary>
    private IUserListRow? _selectedRow;
    public IUserListRow? SelectedRow
    {
        get => _selectedRow;
        set => SetProperty(ref _selectedRow, value);
    }

    public string? LastInvokedVerb { get; private set; }
    public ICommand UserCommand { get; }

    public ChannelViewStub(params UserViewModel[] users)
    {
        UserList = users.ToList();
        UserRows = UserListGrouping.Build(UserList);
        UserCommand = new RelayCommand<string>(verb => LastInvokedVerb = verb);
    }
}
