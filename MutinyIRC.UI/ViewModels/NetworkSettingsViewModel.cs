using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.Common;

namespace MutinyIRC.UI.ViewModels;

public partial class NetworkSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _nick = string.Empty;
    [ObservableProperty] private string _alternateNick = string.Empty;
    [ObservableProperty] private bool _autoConnect;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddServerCommand))]
    private string _newServerText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddChannelCommand))]
    private string _newChannelText = string.Empty;

    /// <summary>Whether the "+ Add channel" affordance has opened its input.</summary>
    [ObservableProperty] private bool _isAddingChannel;

    /// <summary>What the nick fields fall back to when you leave them blank. Shown as the
    /// watermark, so an empty field says which nick it will actually use.</summary>
    public static string NickWatermark => AppSettings.Instance.FirstNick;

    /// <inheritdoc cref="NickWatermark"/>
    public static string AlternateNickWatermark => AppSettings.Instance.SecondNick;

    /// <summary>The "Verify TLS certificates" switch. See <see cref="NetworkSettings.VerifyCertificates"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VerifyCertificatesHint))]
    [NotifyPropertyChangedFor(nameof(IsVerifyCertificatesHintWarning))]
    private bool _verifyCertificates = true;

    internal const string VerifyOnHint =
        "Refuses a server whose certificate fails the check, unless you pinned that certificate";

    internal const string VerifyOffHint =
        "Off: Mutiny accepts any certificate from this network's servers";

    internal const string NoTlsHint =
        "None of this network's servers use TLS, so there is no certificate to check.";

    /// <summary>
    /// Whether the verify switch can do anything. A plaintext entry point has no certificate,
    /// so the switch is disabled when no entry point uses TLS.
    /// </summary>
    public bool CanVerifyCertificates => Servers.Any(server => server.Ssl);

    /// <summary>The line under the verify switch that says what it does now.</summary>
    public string VerifyCertificatesHint =>
        !CanVerifyCertificates ? NoTlsHint
        : VerifyCertificates ? VerifyOnHint
        : VerifyOffHint;

    /// <summary>
    /// Whether <see cref="VerifyCertificatesHint"/> is a warning: the switch is off for a
    /// network that has a TLS entry point.
    /// </summary>
    public bool IsVerifyCertificatesHintWarning => CanVerifyCertificates && !VerifyCertificates;

    /// <summary>
    /// The mark that the pin action minted this network. The dialog does not show or change it,
    /// but a save must keep it. See <see cref="NetworkSettings.MintedByPin"/>.
    /// </summary>
    public bool MintedByPin { get; }

    public ObservableCollection<ServerSettingsViewModel> Servers { get; } = new();
    public ObservableCollection<ChannelSettingsViewModel> Channels { get; } = new();

    public NetworkSettingsViewModel()
    {
        // The entry points decide whether the switch applies. A row cannot change its TLS
        // flag, so only an add or a remove can change the answer.
        Servers.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanVerifyCertificates));
            OnPropertyChanged(nameof(VerifyCertificatesHint));
            OnPropertyChanged(nameof(IsVerifyCertificatesHintWarning));
        };
    }

    public NetworkSettingsViewModel(NetworkSettings model) : this()
    {
        _name = model.Name ?? string.Empty;
        _nick = model.Nick ?? string.Empty;
        _alternateNick = model.AlternateNick ?? string.Empty;
        _autoConnect = model.AutoConnect;
        _verifyCertificates = model.VerifyCertificates;
        MintedByPin = model.MintedByPin;
        foreach (var server in model.Servers)
            Servers.Add(new ServerSettingsViewModel(server));
        foreach (var channel in model.Channels)
            Channels.Add(new ChannelSettingsViewModel(channel));
        RenumberServers();
    }

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

    /// <summary>
    /// Reads a host, or a host and a port, out of what the user typed. Returns null for anything
    /// that does not parse, which is what leaves the Add button disabled.
    /// </summary>
    /// <remarks>
    /// <see cref="EntryPoint.TryParse"/> holds the rules, so this box and <c>/server</c> accept
    /// the same input.
    /// </remarks>
    public static ServerSettingsViewModel? ParseServer(string? text)
    {
        if (text is null || !EntryPoint.TryParse(text, out EntryPoint entry))
            return null;

        return new ServerSettingsViewModel
        {
            Url = entry.Host,
            Port = entry.Port,
            Ssl = entry.Ssl,
        };
    }

    /// <summary>
    /// Reads a channel, and its key where one is given, out of what the user typed. A name with
    /// no channel prefix gets a '#', because that is what a user who types "mutinyirc" means.
    /// </summary>
    public static ChannelSettingsViewModel? ParseChannel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        string[] parts = text.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return null;

        string name = parts[0];
        if (name.Length == 0 || name is "#" or "&")
            return null;

        if (!name.StartsWith('#') && !name.StartsWith('&'))
            name = "#" + name;

        return new ChannelSettingsViewModel
        {
            Name = name,
            Key = parts.Length > 1 ? parts[1].Trim() : string.Empty,
        };
    }

    [RelayCommand(CanExecute = nameof(CanAddServer))]
    private void AddServer()
    {
        ServerSettingsViewModel? parsed = ParseServer(NewServerText);
        if (parsed is null)
            return;

        Servers.Add(parsed);
        RenumberServers();
        NewServerText = string.Empty;
    }

    private bool CanAddServer() => ParseServer(NewServerText) is not null;

    [RelayCommand]
    private void RemoveServer(ServerSettingsViewModel? server)
    {
        if (server is null)
            return;

        Servers.Remove(server);
        RenumberServers();
    }

    [RelayCommand]
    private void MoveServerUp(ServerSettingsViewModel? server) => MoveServer(server, -1);

    [RelayCommand]
    private void MoveServerDown(ServerSettingsViewModel? server) => MoveServer(server, 1);

    /// <summary>
    /// Moves an entry point along the failover order. A move off either end does nothing, so the
    /// view can bind the buttons without also having to disable them.
    /// </summary>
    private void MoveServer(ServerSettingsViewModel? server, int delta)
    {
        if (server is null)
            return;

        int from = Servers.IndexOf(server);
        int to = from + delta;
        if (from < 0 || to < 0 || to >= Servers.Count)
            return;

        Servers.Move(from, to);
        RenumberServers();
    }

    /// <summary>
    /// Rewrites every row's place in the order. A row cannot see the list it is in, so the list
    /// tells it, once per change.
    /// </summary>
    private void RenumberServers()
    {
        for (int i = 0; i < Servers.Count; i++)
        {
            Servers[i].Order = i + 1;
            Servers[i].IsFirst = i == 0;
        }
    }

    [RelayCommand]
    private void BeginAddChannel()
    {
        NewChannelText = string.Empty;
        IsAddingChannel = true;
    }

    [RelayCommand(CanExecute = nameof(CanAddChannel))]
    private void AddChannel()
    {
        ChannelSettingsViewModel? parsed = ParseChannel(NewChannelText);
        if (parsed is null)
            return;

        Channels.Add(parsed);
        NewChannelText = string.Empty;
        IsAddingChannel = false;
    }

    private bool CanAddChannel() => ParseChannel(NewChannelText) is not null;

    [RelayCommand]
    private void CancelAddChannel()
    {
        NewChannelText = string.Empty;
        IsAddingChannel = false;
    }

    [RelayCommand]
    private void RemoveChannel(ChannelSettingsViewModel? channel)
    {
        if (channel is not null)
            Channels.Remove(channel);
    }

    public NetworkSettings ToModel()
    {
        var model = new NetworkSettings(Name)
        {
            Nick = string.IsNullOrWhiteSpace(Nick) ? null : Nick,
            AlternateNick = string.IsNullOrWhiteSpace(AlternateNick) ? null : AlternateNick,
            AutoConnect = AutoConnect,
            VerifyCertificates = VerifyCertificates,
            MintedByPin = MintedByPin,
        };

        foreach (var vm in Servers)
            model.AddServer(vm.ToModel());
        foreach (var vm in Channels)
            model.AddChannel(vm.ToModel());
        return model;
    }
}
