using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MutinyIRC.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IrcSettingsManager _settings;

    [ObservableProperty] private NetworkSettingsViewModel? _selectedNetwork;

    public ObservableCollection<NetworkSettingsViewModel> Networks { get; } = new();

    /// <summary>
    /// Occurs during <see cref="Save"/>, after the dialog and the held live writes are applied
    /// to the saved networks, and before the settings go to disk.
    /// </summary>
    public event EventHandler? NetworksApplied;

    public SettingsViewModel() : this(IrcSettingsManager.Instance) { }

    /// <summary>
    /// Edits the networks of <paramref name="settings"/>. Taking the manager rather than reaching
    /// the singleton is what lets a test run this view model without touching the user's
    /// servers.json.
    /// </summary>
    public SettingsViewModel(IrcSettingsManager settings)
    {
        _settings = settings;

        // The rows are a copy of the networks. The live writes wait until the dialog closes, so
        // that the copy stays the same as the networks.
        _settings.HoldLiveWrites();

        // The rule watches Networks and gives itself to each network in it, so those
        // references keep it alive. Networks added later, also by a subclass, get it too.
        _ = new ListedHosts(Networks);

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

    /// <summary>
    /// The OK of the dialog. All on the UI thread, in this order: it writes the dialog onto the
    /// saved networks in place, applies the live writes that waited, gives each live connection
    /// the new entry points of its network, and then writes the settings to disk once.
    /// </summary>
    /// <remarks>
    /// A live write that waited obeys the edits of the dialog: see <see cref="DialogEdits"/> and
    /// <see cref="IrcSettingsManager.WhenWritable"/>.
    /// </remarks>
    public void Save()
    {
        DialogEdits edits = DialogEdits.Compare(Networks, _settings.Networks);
        ApplyToSavedNetworks();
        _settings.ApplyHeldWrites(edits);
        GiveConnectionsTheirEntryPoints();
        _settings.Save();
    }

    /// <summary>
    /// The Cancel of the dialog. It applies the live writes that waited as they are, in memory
    /// only, and does not write to disk. After <see cref="Save"/>, it does nothing.
    /// </summary>
    public void Cancel() => _settings.ApplyHeldWrites(DialogEdits.None);

    /// <summary>
    /// Writes the dialog onto the saved networks in place.
    /// </summary>
    /// <remarks>
    /// A network that was in the dialog before and after the save is the same object as before,
    /// so a live connection that holds it keeps it. A network added in the dialog becomes a new
    /// object, and a network deleted in the dialog is removed.
    /// </remarks>
    private void ApplyToSavedNetworks()
    {
        NetworkSettingsList saved = _settings.Networks;

        // Remove the deleted networks first. Then a host that moved from a deleted network to
        // another network is not claimed twice.
        var kept = new HashSet<NetworkSettings>(
            Networks.Select(vm => vm.Source).OfType<NetworkSettings>(),
            ReferenceEqualityComparer.Instance);
        foreach (NetworkSettings network in saved.ToList())
            if (!kept.Contains(network))
                saved.Remove(network);

        // A hostname belongs to one network. The first row that lists a host keeps it, and a
        // later row drops it, as NetworkSettingsList.ReplaceAll does for data from disk.
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var added = new List<NetworkSettings>();
        foreach (NetworkSettingsViewModel vm in Networks)
        {
            NetworkSettings network = vm.Source ?? new NetworkSettings();
            vm.ApplyTo(network, server => claimed.Add(server.Url));
            if (vm.Source is null)
                added.Add(network);
        }

        // The dialog adds a network only at the end, so an added network goes after the others.
        foreach (NetworkSettings network in added)
        {
            bool accepted = saved.TryAdd(network);
            Debug.Assert(accepted, "The save drops every host that another network claims");
        }
    }

    /// <summary>
    /// Gives each live connection the new entry point list of its bound network. It comes after
    /// the held writes, because a held merge can move an entry point, and before the disk write.
    /// The owner of the connections does the hand-off when <see cref="NetworksApplied"/> occurs.
    /// </summary>
    private void GiveConnectionsTheirEntryPoints()
    {
        NetworksApplied?.Invoke(this, EventArgs.Empty);
    }
}
