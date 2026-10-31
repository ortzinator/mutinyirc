using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.Common;
using MutinyIRC.UI.Resources;

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

    /// <summary>
    /// The refusal under the Add box when the dialog already lists the typed host, or empty
    /// when it does not. See <see cref="DescribeClash"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNewServerClash))]
    [NotifyCanExecuteChangedFor(nameof(AddServerCommand))]
    private string _newServerClash = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddChannelCommand))]
    private string _newChannelText = string.Empty;

    /// <summary>
    /// The status line under the entry point list after a move out of this network, or empty.
    /// The next action in this network clears it. See <see cref="MoveServerTo"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMoveStatus))]
    private string _moveStatus = string.Empty;

    private ListedHosts? _hosts;

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

    /// <summary>
    /// Whether the verify switch can do anything. A plaintext entry point has no certificate,
    /// so the switch is disabled when no entry point uses TLS.
    /// </summary>
    public bool CanVerifyCertificates => Servers.Any(server => server.Ssl);

    /// <summary>The line under the verify switch that says what it does now.</summary>
    public string VerifyCertificatesHint =>
        !CanVerifyCertificates ? ServerStrings.VerifyCertificatesNoTlsHint
        : VerifyCertificates ? ServerStrings.VerifyCertificatesOnHint
        : ServerStrings.VerifyCertificatesOffHint;

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

    /// <summary>
    /// The saved network that this row is a copy of, or null for a network added in the dialog.
    /// A save writes onto this object, so that a holder of the network keeps it after the save.
    /// </summary>
    public NetworkSettings? Source { get; }

    public ObservableCollection<ServerSettingsViewModel> Servers { get; } = new();
    public ObservableCollection<ChannelSettingsViewModel> Channels { get; } = new();

    /// <summary>Whether <see cref="NewServerClash"/> has a refusal to show.</summary>
    public bool HasNewServerClash => NewServerClash.Length > 0;

    /// <summary>Whether <see cref="MoveStatus"/> has a line to show.</summary>
    public bool HasMoveStatus => MoveStatus.Length > 0;

    /// <summary>
    /// The networks that an entry point of this network can move to: every other network of the
    /// dialog, in the order that the dialog shows them. Empty when no dialog shows this network.
    /// </summary>
    public IReadOnlyList<NetworkSettingsViewModel> MoveTargets =>
        _hosts is null
            ? Array.Empty<NetworkSettingsViewModel>()
            : _hosts.Networks.Where(network => network != this).ToList();

    /// <summary>
    /// Occurs when a host of this network can have a different holder: a row was added or
    /// removed, a row changed its host, or the network changed its name.
    /// </summary>
    internal event EventHandler? HostsChanged;

    /// <summary>
    /// The host rule of the dialog that shows this network. The dialog sets it. When it is null,
    /// the rule reads only the rows of this network.
    /// </summary>
    internal ListedHosts? Hosts
    {
        get => _hosts;
        set
        {
            if (_hosts == value)
                return;

            if (_hosts is not null)
                _hosts.Changed -= OnListedHostsChanged;
            _hosts = value;
            if (_hosts is not null)
                _hosts.Changed += OnListedHostsChanged;
            CheckHosts();
            OnPropertyChanged(nameof(MoveTargets));
        }
    }

    public NetworkSettingsViewModel()
    {
        Servers.CollectionChanged += OnServersChanged;
        Channels.CollectionChanged += (_, _) => ClearMoveStatus();
    }

    public NetworkSettingsViewModel(NetworkSettings model) : this()
    {
        _name = model.Name ?? string.Empty;
        _nick = model.Nick ?? string.Empty;
        _alternateNick = model.AlternateNick ?? string.Empty;
        _autoConnect = model.AutoConnect;
        _verifyCertificates = model.VerifyCertificates;
        MintedByPin = model.MintedByPin;
        Source = model;
        foreach (var server in model.Servers)
            Servers.Add(new ServerSettingsViewModel(server));
        foreach (var channel in model.Channels)
            Channels.Add(new ChannelSettingsViewModel(channel));
        RenumberServers();
    }

    public void BeginEdit()
    {
        ClearMoveStatus();
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

    /// <summary>
    /// Moves an entry point of this network to the end of the failover order of
    /// <paramref name="target"/>. The entry point keeps its pin, and this network keeps its
    /// settings, also when no entry point remains. The move sets <see cref="MoveStatus"/>.
    /// </summary>
    /// <remarks>
    /// The move does no host check. The dialog lists a host at most once, so the target cannot
    /// list it already. <see cref="MintedByPin"/> does not change on either network.
    /// </remarks>
    /// <param name="server">An entry point of this network. Another row does nothing.</param>
    /// <param name="target">Another network of the dialog. This network does nothing.</param>
    public void MoveServerTo(ServerSettingsViewModel? server, NetworkSettingsViewModel? target)
    {
        if (server is null || target is null || target == this || !Servers.Contains(server))
            return;

        server.CloseEditor();
        Servers.Remove(server);
        RenumberServers();
        target.Servers.Add(server);
        target.RenumberServers();
        MoveStatus = ServerStrings.EntryPointMoved.With(server.Url, target.Name);
    }

    private void ClearMoveStatus() => MoveStatus = string.Empty;

    private bool CanAddServer() => ParseServer(NewServerText) is not null && !HasNewServerClash;

    /// <summary>
    /// Whether a row of this network has <paramref name="host"/>, compared without case.
    /// </summary>
    /// <param name="host">The hostname to find.</param>
    /// <param name="except">A row that does not count.</param>
    internal bool Lists(string host, ServerSettingsViewModel? except = null) =>
        Servers.Any(server => server != except && EntryPoint.SameHost(server.Url, host));

    /// <summary>
    /// Gets the refusal for a host that the dialog already lists, from the view of this network.
    /// Returns empty when no row lists <paramref name="host"/>.
    /// </summary>
    /// <param name="host">The hostname to check.</param>
    /// <param name="except">A row that does not count. A row is never a clash with itself.</param>
    public string DescribeClash(string host, ServerSettingsViewModel? except = null)
    {
        NetworkSettingsViewModel? holder = _hosts is not null
            ? _hosts.FindHolder(host, except)
            : Lists(host, except) ? this : null;

        if (holder is null)
            return string.Empty;

        return holder == this
            ? ServerStrings.HostListedInThisNetwork.With(host)
            : ServerStrings.HostListedInNetwork.With(host, holder.Name);
    }

    // Sets the refusal under the Add box for the text in it now.
    private void CheckNewServer()
    {
        ServerSettingsViewModel? parsed = ParseServer(NewServerText);
        NewServerClash = parsed is null ? string.Empty : DescribeClash(parsed.Url);
    }

    partial void OnNewServerTextChanged(string value) => CheckNewServer();

    partial void OnNameChanged(string value) => OnHostsChanged();

    // The fields of this network that the user types in or switches. A change of one is an
    // action in this network, so it clears the status line.
    private static readonly HashSet<string> UserFields = new()
    {
        nameof(Name), nameof(Nick), nameof(AlternateNick), nameof(AutoConnect),
        nameof(VerifyCertificates), nameof(NewServerText),
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is { } name && UserFields.Contains(name))
            ClearMoveStatus();
    }

    // A network can be added, removed or renamed, so the networks to move to can change too.
    private void OnListedHostsChanged(object? sender, EventArgs e)
    {
        CheckHosts();
        OnPropertyChanged(nameof(MoveTargets));
    }

    // Sets every refusal of this network for the rows of the dialog now: the one under the Add
    // box, and the one in each open row editor.
    private void CheckHosts()
    {
        CheckNewServer();
        foreach (ServerSettingsViewModel server in Servers)
            server.Editor?.CheckClash();
    }

    private void OnHostsChanged()
    {
        HostsChanged?.Invoke(this, EventArgs.Empty);

        // With a dialog, the dialog tells every network, this network too.
        if (_hosts is null)
            CheckHosts();
    }

    private void OnServersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (ServerSettingsViewModel server in e.OldItems)
                server.PropertyChanged -= OnServerPropertyChanged;
        if (e.NewItems is not null)
            foreach (ServerSettingsViewModel server in e.NewItems)
                server.PropertyChanged += OnServerPropertyChanged;

        ClearMoveStatus();
        OnTlsChanged();
        OnHostsChanged();
    }

    private void OnServerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The network writes the place of a row in the order. That is not an action of the user.
        if (e.PropertyName is not (nameof(ServerSettingsViewModel.Order)
                                   or nameof(ServerSettingsViewModel.IsFirst)))
            ClearMoveStatus();

        if (e.PropertyName == nameof(ServerSettingsViewModel.Url))
            OnHostsChanged();
        else if (e.PropertyName == nameof(ServerSettingsViewModel.Ssl))
            OnTlsChanged();
    }

    // The TLS flags of the entry points decide whether the verify switch applies. An add, a
    // remove, or an edit of a TLS flag can change the answer. The switch keeps its own value.
    private void OnTlsChanged()
    {
        OnPropertyChanged(nameof(CanVerifyCertificates));
        OnPropertyChanged(nameof(VerifyCertificatesHint));
        OnPropertyChanged(nameof(IsVerifyCertificatesHintWarning));
    }

    /// <summary>
    /// Opens the editor of an entry point row. The editor refuses a host that another row of
    /// the dialog lists. See <see cref="DescribeClash"/>.
    /// </summary>
    [RelayCommand]
    private void EditServer(ServerSettingsViewModel? server)
    {
        if (server is null || !Servers.Contains(server))
            return;

        server.OpenEditor(host => DescribeClash(host, server));
    }

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
        ClearMoveStatus();
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
        var model = new NetworkSettings { MintedByPin = MintedByPin };
        ApplyTo(model, _ => true);
        return model;
    }

    /// <summary>
    /// Writes the fields of this row onto <paramref name="model"/>. The entry point list and the
    /// channel list are replaced as a whole, because nothing live holds them by reference.
    /// </summary>
    /// <param name="model">The network to write onto. The object stays the same.</param>
    /// <param name="keepServer">
    /// Says if an entry point goes into the list. An entry point it refuses is not saved.
    /// </param>
    /// <remarks>
    /// <see cref="NetworkSettings.MintedByPin"/> is not written. The dialog does not edit the
    /// mark, so the network keeps its own value.
    /// </remarks>
    public void ApplyTo(NetworkSettings model, Func<ServerSettings, bool> keepServer)
    {
        model.Name = Name;
        model.Nick = string.IsNullOrWhiteSpace(Nick) ? null : Nick;
        model.AlternateNick = string.IsNullOrWhiteSpace(AlternateNick) ? null : AlternateNick;
        model.AutoConnect = AutoConnect;
        model.VerifyCertificates = VerifyCertificates;

        model.Servers = new List<ServerSettings>();
        foreach (var vm in Servers)
        {
            ServerSettings server = vm.ToModel();
            if (keepServer(server))
                model.AddServer(server);
        }

        model.Channels = new List<ChannelSettings>();
        foreach (var vm in Channels)
            model.AddChannel(vm.ToModel());
    }
}
