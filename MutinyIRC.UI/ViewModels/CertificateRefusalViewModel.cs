using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// The error block for a refused server certificate, with the button that pins the certificate
/// on the refused entry point. Only the newest refusal of a connection can trust: the server
/// window sets <see cref="CanTrust"/> to false on older refusals.
/// </summary>
public partial class CertificateRefusalViewModel : ErrorMessageViewModel
{
    private readonly Func<CertificateRefusalViewModel, Task> _trust;

    /// <param name="message">The refusal text from <see cref="CertificateRefusal.Describe"/>.</param>
    /// <param name="host">The host of the refused entry point.</param>
    /// <param name="port">The port of the refused entry point.</param>
    /// <param name="fingerprint">The SHA-256 fingerprint of the refused certificate, as hex.</param>
    /// <param name="pinnedFingerprint">The pin of the entry point, or null when it has none.</param>
    /// <param name="trust">What the button does.</param>
    public CertificateRefusalViewModel(DateTime time, string message, string host, int port,
                                       string fingerprint, string? pinnedFingerprint,
                                       Func<CertificateRefusalViewModel, Task> trust)
        : base(time, message)
    {
        Host = host;
        Port = port;
        Fingerprint = fingerprint;
        PinnedFingerprint = pinnedFingerprint;
        _trust = trust;
    }

    public string Host { get; }

    public int Port { get; }

    /// <summary>The SHA-256 fingerprint of the refused certificate, as hex.</summary>
    public string Fingerprint { get; }

    /// <summary>The pin that the refused certificate does not match, or null.</summary>
    public string? PinnedFingerprint { get; }

    /// <summary>
    /// Whether the entry point already has a pin, so that the button replaces it, after a
    /// confirm.
    /// </summary>
    public bool ReplacesPin => PinnedFingerprint is not null;

    public string TrustLabel => ReplacesPin
        ? ServerStrings.ReplacePinnedCertificateButton
        : ServerStrings.TrustCertificateButton;

    /// <summary>
    /// Whether the button shows and works. It is false when a newer refusal came, when the
    /// connection registered, or when the button was used.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrustCommand))]
    private bool _canTrust = true;

    [RelayCommand(CanExecute = nameof(CanTrust))]
    private Task Trust() => _trust(this);
}
