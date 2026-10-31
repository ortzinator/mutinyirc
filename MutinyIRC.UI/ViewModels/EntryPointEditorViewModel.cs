using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.Common;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// The editor that an entry point row becomes: a draft of its host, port and TLS flag. The row
/// does not change until Apply. Cancel discards the draft.
/// </summary>
/// <remarks>
/// The TLS flag is a field of its own. It never rewrites the port. A blank port becomes the
/// default port for the TLS flag on Apply: 6697 with TLS, 6667 without.
/// </remarks>
public sealed partial class EntryPointEditorViewModel : ObservableObject
{
    private readonly ServerSettingsViewModel _row;

    // Gets the refusal for a host from the network of the row, or empty when no other row
    // lists the host. The row itself does not count.
    private readonly Func<string, string> _describeClash;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private string _host;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private string _port;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinWillClear))]
    [NotifyPropertyChangedFor(nameof(PortPlaceholder))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private bool _ssl;

    /// <summary>
    /// The refusal under the fields when the dialog already lists the draft host, or empty
    /// when it does not.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRefusal))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private string _refusal = string.Empty;

    internal EntryPointEditorViewModel(ServerSettingsViewModel row, Func<string, string> describeClash)
    {
        _row = row;
        _describeClash = describeClash;
        _host = row.Url;
        _port = row.Port.ToString(CultureInfo.InvariantCulture);
        _ssl = row.Ssl;
        CheckClash();
    }

    /// <summary>Whether <see cref="Refusal"/> has a refusal to show.</summary>
    public bool HasRefusal => Refusal.Length > 0;

    /// <summary>The port that a blank port field becomes, for the TLS flag of the draft.</summary>
    public string PortPlaceholder => DefaultPort.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Whether Apply forgets the pin of the row: the row has a pin, and the draft has a new
    /// host or port, or TLS off. A change of case in the host keeps the pin.
    /// </summary>
    public bool PinWillClear =>
        _row.IsPinned
        && (!string.Equals(Host.Trim(), _row.Url, StringComparison.OrdinalIgnoreCase)
            || ParsePort() != _row.Port
            || !Ssl);

    private int DefaultPort => Ssl ? IrcPorts.DefaultTlsPort : IrcPorts.DefaultPlainPort;

    /// <summary>Sets <see cref="Refusal"/> for the draft host and the rows of the dialog now.</summary>
    internal void CheckClash()
    {
        string host = Host.Trim();
        Refusal = IsValidHost(host) ? _describeClash(host) : string.Empty;
    }

    internal void OnPinChanged() => OnPropertyChanged(nameof(PinWillClear));

    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply()
    {
        string host = Host.Trim();
        int? port = ParsePort();
        if (!IsValidHost(host) || port is null || HasRefusal)
            return;

        _row.Apply(host, port.Value, Ssl);
    }

    private bool CanApply() => IsValidHost(Host.Trim()) && ParsePort() is not null && !HasRefusal;

    [RelayCommand]
    private void Cancel() => _row.CloseEditor();

    partial void OnHostChanged(string value) => CheckClash();

    // Gets the port that the draft gives, or null when the port field is not a valid port.
    private int? ParsePort()
    {
        string text = Port.Trim();
        if (text.Length == 0)
            return DefaultPort;

        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int port)
               && IrcPorts.IsValidPort(port)
            ? port
            : null;
    }

    // A host is valid when the Add box would read it as a host with no port.
    private static bool IsValidHost(string host) =>
        EntryPoint.TryParse(host, out EntryPoint entry) && entry.Host == host;
}
