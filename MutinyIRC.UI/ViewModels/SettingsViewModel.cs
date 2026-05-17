namespace MutinyIRC.UI.ViewModels;

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private NetworkSettingsViewModel? _selectedNetwork;

    public ObservableCollection<NetworkSettingsViewModel> Networks { get; } = new();

    public SettingsViewModel()
    {
        foreach (var network in IrcSettingsManager.Instance.Networks)
            Networks.Add(new NetworkSettingsViewModel(network));
        SelectedNetwork = Networks.Count > 0 ? Networks[0] : null;
        if (SelectedNetwork is not null && SelectedNetwork.Servers.Count > 0)
            SelectedNetwork.SelectedServer = SelectedNetwork.Servers[0];
    }

    [RelayCommand]
    private void AddNetwork()
    {
        var vm = new NetworkSettingsViewModel { Name = "New Network" };
        Networks.Add(vm);
        SelectedNetwork = vm;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveNetwork))]
    private void RemoveNetwork()
    {
        if (SelectedNetwork is null) return;
        int idx = Networks.IndexOf(SelectedNetwork);
        Networks.Remove(SelectedNetwork);
        SelectedNetwork = Networks.Count > 0
            ? Networks[Math.Min(idx, Networks.Count - 1)]
            : null;
    }

    private bool CanRemoveNetwork() => SelectedNetwork is not null;

    partial void OnSelectedNetworkChanged(NetworkSettingsViewModel? value)
        => RemoveNetworkCommand.NotifyCanExecuteChanged();

    public void Save()
    {
        IrcSettingsManager.Instance.Networks.Clear();
        foreach (var vm in Networks)
            IrcSettingsManager.Instance.Networks.Add(vm.ToModel());
        IrcSettingsManager.Instance.Save();
    }
}
