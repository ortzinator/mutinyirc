namespace MutinyIRC.UI.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class NetworkSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ServerSettingsViewModel? _selectedServer;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editName = string.Empty;

    public void BeginEdit()
    {
        EditName = Name;
        IsEditing = true;
    }

    [RelayCommand]
    private void CommitEdit()
    {
        Name = EditName;
        IsEditing = false;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditing = false;

    public ObservableCollection<ServerSettingsViewModel> Servers { get; } = new();

    public NetworkSettingsViewModel() { }

    public NetworkSettingsViewModel(NetworkSettings model)
    {
        _name = model.Name ?? string.Empty;
        foreach (var server in model.Servers)
            Servers.Add(new ServerSettingsViewModel(server));
    }

    [RelayCommand]
    private void AddServer()
    {
        var vm = new ServerSettingsViewModel { AutoConnect = true };
        Servers.Add(vm);
        SelectedServer = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveServer))]
    private void RemoveServer()
    {
        if (SelectedServer is null) return;
        int idx = Servers.IndexOf(SelectedServer);
        Servers.Remove(SelectedServer);
        SelectedServer = Servers.Count > 0
            ? Servers[Math.Min(idx, Servers.Count - 1)]
            : null;
    }

    private bool CanRemoveServer() => SelectedServer is not null;

    partial void OnSelectedServerChanged(ServerSettingsViewModel? value)
        => RemoveServerCommand.NotifyCanExecuteChanged();

    public NetworkSettings ToModel()
    {
        var model = new NetworkSettings(Name);
        foreach (var vm in Servers)
            model.AddServer(vm.ToModel());
        return model;
    }
}
