using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MutinyIRC.UI.ViewModels;

public partial class NetworkSettingsViewModel : ObservableObject
{
    /// <summary>A bare host, or a host and a port. Ported from the mockup's `parseServer()`.</summary>
    private static readonly Regex HostPattern =
        new(@"^([A-Za-z0-9.\-_]+)(?::(\d{2,5}))?$", RegexOptions.Compiled);

    /// <summary>
    /// The port to assume when the user names only a host. TLS is the sensible default for a
    /// network you are adding today.
    /// </summary>
    public const int DefaultNewServerPort = 6697;

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

    public ObservableCollection<ServerSettingsViewModel> Servers { get; } = new();
    public ObservableCollection<ChannelSettingsViewModel> Channels { get; } = new();

    public NetworkSettingsViewModel() { }

    public NetworkSettingsViewModel(NetworkSettings model)
    {
        _name = model.Name ?? string.Empty;
        _nick = model.Nick ?? string.Empty;
        _alternateNick = model.AlternateNick ?? string.Empty;
        _autoConnect = model.AutoConnect;
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
    /// The port decides TLS: the two ports that historically mean plaintext stay plaintext, and
    /// everything else is assumed to be TLS. Ports 6667 and 6665 come from the mockup.
    /// </remarks>
    public static ServerSettingsViewModel? ParseServer(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        Match match = HostPattern.Match(text.Trim());
        if (!match.Success)
            return null;

        int port = DefaultNewServerPort;
        if (match.Groups[2].Success && !int.TryParse(match.Groups[2].Value, out port))
            return null;

        if (port <= 0 || port > 65535)
            return null;

        return new ServerSettingsViewModel
        {
            Url = match.Groups[1].Value,
            Port = port,
            Ssl = port != 6667 && port != 6665,
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
        };

        foreach (var vm in Servers)
            model.AddServer(vm.ToModel());
        foreach (var vm in Channels)
            model.AddChannel(vm.ToModel());
        return model;
    }
}
