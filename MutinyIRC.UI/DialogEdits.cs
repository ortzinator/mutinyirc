using System;
using System.Collections.Generic;
using System.Linq;
using MutinyIRC.Common;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI;

/// <summary>
/// What an OK of the Networks dialog changed on the saved networks. The held live writes read it
/// when they apply after the save, so that the edit of the user wins over a held write.
/// </summary>
internal sealed class DialogEdits
{
    /// <summary>No edits: Cancel, and a live write when the dialog is not open.</summary>
    internal static DialogEdits None { get; } = new();

    private readonly HashSet<NetworkSettings> _deleted = NewNetworkSet();
    private readonly HashSet<NetworkSettings> _renamed = NewNetworkSet();
    private readonly HashSet<NetworkSettings> _autoConnectChanged = NewNetworkSet();
    private readonly HashSet<NetworkSettings> _entryPointsChanged = NewNetworkSet();

    private DialogEdits() { }

    private static HashSet<NetworkSettings> NewNetworkSet() =>
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Finds the edits: it compares the rows of the dialog with the saved networks. Call it
    /// before the save writes the rows onto the saved networks.
    /// </summary>
    internal static DialogEdits Find(
        IEnumerable<NetworkSettingsViewModel> rows, IEnumerable<NetworkSettings> saved)
    {
        var edits = new DialogEdits();
        var kept = new HashSet<NetworkSettings>(ReferenceEqualityComparer.Instance);

        foreach (NetworkSettingsViewModel row in rows)
        {
            if (row.Source is not { } network)
                continue;

            kept.Add(network);
            if (row.Name != network.Name)
                edits._renamed.Add(network);
            if (row.AutoConnect != network.AutoConnect)
                edits._autoConnectChanged.Add(network);
            if (!SameEntryPoints(row, network))
                edits._entryPointsChanged.Add(network);
        }

        foreach (NetworkSettings network in saved)
            if (!kept.Contains(network))
                edits._deleted.Add(network);

        return edits;
    }

    private static bool SameEntryPoints(NetworkSettingsViewModel row, NetworkSettings network) =>
        row.Servers.Select(server => server.ToModel())
           .SequenceEqual(network.Servers, EntryPointComparer.Instance);

    /// <summary>Whether the dialog deleted <paramref name="network"/>.</summary>
    internal bool Deleted(NetworkSettings network) => _deleted.Contains(network);

    /// <summary>
    /// Whether the dialog decided the name of <paramref name="network"/>: it deleted the network
    /// or changed its name. Then a reported name must not change it.
    /// </summary>
    internal bool DecidedName(NetworkSettings network) =>
        Deleted(network) || _renamed.Contains(network);

    /// <summary>
    /// Whether the dialog decided the AutoConnect of <paramref name="network"/>: it deleted the
    /// network or changed its AutoConnect. Then a connection must not change it.
    /// </summary>
    internal bool DecidedAutoConnect(NetworkSettings network) =>
        Deleted(network) || _autoConnectChanged.Contains(network);

    /// <summary>
    /// Whether the dialog changed the name or the entry points of <paramref name="network"/>.
    /// </summary>
    internal bool ChangedNameOrEntryPoints(NetworkSettings network) =>
        _renamed.Contains(network) || _entryPointsChanged.Contains(network);

    private sealed class EntryPointComparer : IEqualityComparer<ServerSettings>
    {
        internal static EntryPointComparer Instance { get; } = new();

        public bool Equals(ServerSettings? x, ServerSettings? y) =>
            x is not null && y is not null
            && x.IsAt(y.Url, y.Port)
            && x.Ssl == y.Ssl;

        public int GetHashCode(ServerSettings obj) =>
            HashCode.Combine(
                EntryPoint.HostComparer.GetHashCode(obj.Url), obj.Port, obj.Ssl);
    }
}
