using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// One row of the servers table: a host, its port, and whether that port speaks TLS.
/// </summary>
public partial class ServerSettingsViewModel : ObservableObject
{
    // The number of fingerprint bytes that the pin line shows.
    private const int PinLineBytes = 8;

    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private int _port = ServerSettings.DefaultPort;
    [ObservableProperty] private bool _ssl;

    // The pin of this entry point. ToModel carries it through, so saving the dialog keeps it.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPinned))]
    [NotifyPropertyChangedFor(nameof(PinLine))]
    private string? _pinnedFingerprint;

    // Position in the failover order. The owning network writes both whenever the order
    // changes, because a row cannot see the list it is in. A move keeps the pin.
    [ObservableProperty] private int _order;
    [ObservableProperty] private bool _isFirst;

    public ServerSettingsViewModel() { }

    public ServerSettingsViewModel(ServerSettings model)
    {
        _url = model.Url ?? string.Empty;
        _port = model.Port;
        _ssl = model.Ssl;
        _pinnedFingerprint = model.PinnedFingerprint;
    }

    /// <summary>Whether this entry point has a pinned certificate.</summary>
    public bool IsPinned => PinnedFingerprint is not null;

    /// <summary>
    /// The line under a pinned row: "Pinned" and the first bytes of the fingerprint. Empty when
    /// the entry point has no pin.
    /// </summary>
    public string PinLine
    {
        get
        {
            if (PinnedFingerprint is null)
                return string.Empty;

            string formatted = CertificateRefusal.FormatFingerprint(PinnedFingerprint);
            int length = Math.Min(formatted.Length, PinLineBytes * 3 - 1);
            return $"Pinned {formatted[..length]}…";
        }
    }

    /// <summary>
    /// Clears the pin. The dialog edits a copy of the settings, so the pin stays saved until
    /// the dialog is saved.
    /// </summary>
    [RelayCommand]
    private void ForgetPin() => PinnedFingerprint = null;

    // A pin belongs to one host and port. A different host or port is a different server, so
    // the pin does not apply to it. A host name is not case-sensitive, so a change of case keeps it.
    partial void OnUrlChanged(string? oldValue, string newValue)
    {
        if (!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
            PinnedFingerprint = null;
    }

    partial void OnPortChanged(int value) => PinnedFingerprint = null;

    // A plaintext entry point has no certificate, so it can have no pin.
    partial void OnSslChanged(bool value)
    {
        if (!value)
            PinnedFingerprint = null;
    }

    public ServerSettings ToModel() => new(Url, Port, Ssl) { PinnedFingerprint = PinnedFingerprint };
}
