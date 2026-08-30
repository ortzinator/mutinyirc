using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MutinyIRC.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IrcSettingsManager _settings;

    [ObservableProperty] private NetworkSettingsViewModel? _selectedNetwork;

    public ObservableCollection<NetworkSettingsViewModel> Networks { get; } = new();

    public SettingsViewModel() : this(IrcSettingsManager.Instance) { }

    /// <summary>
    /// Edits the networks of <paramref name="settings"/>. Taking the manager rather than reaching
    /// the singleton is what lets a test run this view model without touching the user's
    /// servers.json.
    /// </summary>
    public SettingsViewModel(IrcSettingsManager settings)
    {
        _settings = settings;
        foreach (var network in _settings.Networks)
            Networks.Add(new NetworkSettingsViewModel(network));
        SelectedNetwork = Networks.Count > 0 ? Networks[0] : null;
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
        // One swap rather than a clear-then-refill, so the settings are never momentarily empty
        // and ReplaceAll gets to hold the one-network-per-hostname rule over the whole set.
        _settings.Networks.ReplaceAll(Networks.Select(vm => vm.ToModel()));
        _settings.Save();
    }
}
