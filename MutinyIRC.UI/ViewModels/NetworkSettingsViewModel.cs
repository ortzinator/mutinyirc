namespace MutinyIRC.UI.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class NetworkSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ServerSettingsViewModel? _selectedServer;
    [ObservableProperty] private ChannelSettingsViewModel? _selectedChannel;
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
    public ObservableCollection<ChannelSettingsViewModel> Channels { get; } = new();

    public NetworkSettingsViewModel() { }

    public NetworkSettingsViewModel(NetworkSettings model)
    {
        _name = model.Name ?? string.Empty;
        foreach (var server in model.Servers)
            Servers.Add(new ServerSettingsViewModel(server));
        foreach (var channel in model.Channels)
            Channels.Add(new ChannelSettingsViewModel(channel));
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

    [RelayCommand]
    private void AddChannel()
    {
        var vm = new ChannelSettingsViewModel { AutoJoin = true };
        Channels.Add(vm);
        SelectedChannel = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveChannel))]
    private void RemoveChannel()
    {
        if (SelectedChannel is null) return;
        int idx = Channels.IndexOf(SelectedChannel);
        Channels.Remove(SelectedChannel);
        SelectedChannel = Channels.Count > 0
            ? Channels[Math.Min(idx, Channels.Count - 1)]
            : null;
    }

    private bool CanRemoveChannel() => SelectedChannel is not null;

    partial void OnSelectedChannelChanged(ChannelSettingsViewModel? value)
        => RemoveChannelCommand.NotifyCanExecuteChanged();

    public NetworkSettings ToModel()
    {
        var model = new NetworkSettings(Name);
        foreach (var vm in Servers)
            model.AddServer(vm.ToModel());
        foreach (var vm in Channels)
            model.AddChannel(vm.ToModel());
        return model;
    }
}