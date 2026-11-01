using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// The rule that the Networks dialog lists a hostname at most once, over all of its networks.
/// The hostname alone is the key, compared without case. The port and the TLS flag do not count.
/// </summary>
/// <remarks>
/// The rule reads the rows of the dialog, not the saved settings. Thus an unsaved add counts at
/// once, and an unsaved delete frees its host at once. Each network that the dialog shows gets
/// this object as its <see cref="NetworkSettingsViewModel.Hosts"/>.
/// </remarks>
internal sealed class ListedHosts
{
    private readonly ObservableCollection<NetworkSettingsViewModel> _networks;
    private readonly HashSet<NetworkSettingsViewModel> _watched = new();

    /// <summary>
    /// Occurs when a host can have a different holder: a network or a row was added or removed,
    /// a row changed its host, or a network changed its name.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>Every network of the dialog, in the order that the dialog shows them.</summary>
    public IReadOnlyList<NetworkSettingsViewModel> Networks => _networks;

    public ListedHosts(ObservableCollection<NetworkSettingsViewModel> networks)
    {
        _networks = networks;
        _networks.CollectionChanged += OnNetworksChanged;
        Sync();
    }

    /// <summary>
    /// Finds the network that lists <paramref name="host"/>, or null when no network lists it.
    /// </summary>
    /// <param name="host">The hostname to find.</param>
    /// <param name="except">A row that does not count. A row is never a clash with itself.</param>
    public NetworkSettingsViewModel? FindHolder(string host, ServerSettingsViewModel? except = null)
    {
        foreach (NetworkSettingsViewModel network in _networks)
            if (network.Lists(host, except))
                return network;
        return null;
    }

    private void OnNetworksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Sync();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Watches each network in the dialog, and stops watching each network that left it. A full
    // compare also covers a Reset, which gives no list of the old items.
    private void Sync()
    {
        foreach (NetworkSettingsViewModel network in new List<NetworkSettingsViewModel>(_watched))
        {
            if (_networks.Contains(network))
                continue;

            _watched.Remove(network);
            network.HostsChanged -= OnNetworkHostsChanged;
            network.Hosts = null;
        }

        foreach (NetworkSettingsViewModel network in _networks)
        {
            if (!_watched.Add(network))
                continue;

            network.HostsChanged += OnNetworkHostsChanged;
            network.Hosts = this;
        }
    }

    private void OnNetworkHostsChanged(object? sender, EventArgs e) =>
        Changed?.Invoke(this, EventArgs.Empty);
}
