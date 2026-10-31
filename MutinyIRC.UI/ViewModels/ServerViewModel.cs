using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using System;
using Avalonia.Threading;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

public class ServerViewModel : IrcViewModel
{
    private int _nickRetryAttempt;
    private bool _nickRetryFailed;
    // IDE0370 is a false positive here: without the '!' the build gives CS8625.
#pragma warning disable IDE0370
    private readonly Server _server = null!;
    private readonly PluginManager _pluginManager = null!;
#pragma warning restore IDE0370

    public MTObservableCollection<ChannelViewModel> Channels { get; } = new MTObservableCollection<ChannelViewModel>();
    public MTObservableCollection<PrivateMessageViewModel> PrivateMessages { get; } = new MTObservableCollection<PrivateMessageViewModel>();
    public override Server? OwningServer => _server;

    private bool _isAway;
    /// <summary>
    /// Whether the local user is marked away on this connection, mirroring
    /// <see cref="Server.IsAway"/>. Bound by the sidebar to show an "(away)" badge.
    /// </summary>
    public bool IsAway
    {
        get => _isAway;
        private set => SetProperty(ref _isAway, value);
    }

    private string _networkName = string.Empty;
    /// <summary>
    /// The network this connection belongs to, as the network switcher names it. Until the
    /// connection registers, the only name we have for it is the host we dialled.
    /// </summary>
    public string NetworkName
    {
        get => _networkName;
        private set => SetProperty(ref _networkName, value);
    }

    private string _host = string.Empty;
    /// <summary>
    /// The host this connection was dialled on. The console row is labelled with it, because
    /// the switcher above already carries the network name.
    /// </summary>
    public string Host
    {
        get => _host;
        private set => SetProperty(ref _host, value);
    }

    private bool _isShown;
    /// <summary>
    /// Whether the sidebar is showing this connection's panels. Only one connection is shown at
    /// a time; <see cref="MainViewModel.ShownServer"/> owns which.
    /// </summary>
    public bool IsShown
    {
        get => _isShown;
        internal set => SetProperty(ref _isShown, value);
    }

    private string _connectionStatus = string.Empty;
    /// <summary>
    /// One short line on where the connection stands, for the network switcher: connected
    /// (noting TLS), connecting, waiting to reconnect, or down.
    /// </summary>
    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set
        {
            SetProperty(ref _connectionStatus, value);
            // A new status is a plain one until something says otherwise.
            IsConnectionStatusWarning = false;
        }
    }

    private bool _isConnectionStatusWarning;
    /// <summary>
    /// Whether the sidebar draws <see cref="ConnectionStatus"/> in the warning colour, for
    /// example for a connection that did not check its certificate.
    /// </summary>
    public bool IsConnectionStatusWarning
    {
        get => _isConnectionStatusWarning;
        private set => SetProperty(ref _isConnectionStatusWarning, value);
    }

    /// <summary>
    /// Whether anything on this connection has unread activity — the server window itself or
    /// any of its channel and private-message panels. The switcher marks a connection you are
    /// not looking at with this.
    /// </summary>
    public bool HasUnreadAnywhere =>
        HasUnread
        || Channels.Any(c => c.HasUnread)
        || PrivateMessages.Any(p => p.HasUnread);

    // The away message last shown for each nick, so messaging an away user repeatedly doesn't
    // re-print their away text. A changed message shows again; cleared when the connection drops.
    private readonly Dictionary<string, string> _shownAwayReplies = new(StringComparer.OrdinalIgnoreCase);

    public ServerViewModel(Server newServer, PluginManager pluginManager)
    {
        if (Avalonia.Controls.Design.IsDesignMode)
            return;

        _pluginManager = pluginManager;
        _server = newServer;
        Name = _server.Url;
        Host = _server.Url;
        NetworkName = _server.Url;
        ConnectionStatus = _server.IsConnected ? ConnectedStatus() : ServerStrings.StatusDisconnected;

        // The unread mark on the switcher covers the whole connection, so it has to follow
        // every panel this connection owns, including ones opened later.
        Channels.CollectionChanged += UnreadSources_CollectionChanged;
        PrivateMessages.CollectionChanged += UnreadSources_CollectionChanged;

        _server.Registered += Server_Registered;
        _server.NetworkNameReported += Server_NetworkNameReported;
        _server.ConnectFailed += Server_ConnectFailed;
        _server.Reconnecting += Server_Reconnecting;
        _server.EveryEntryPointRefused += Server_EveryEntryPointRefused;
        _server.NoEntryPoints += Server_NoEntryPoints;
        _server.ErrorMessageRecieved += Server_ErrorMessageRecieved;
        _server.Connecting += Server_Connecting;
        _server.Disconnected += Server_Disconnected;
        _server.ConnectionLost += Server_ConnectionLost;
        _server.ConnectCancelled += Server_ConnectCancelled;
        _server.NickError += Server_NickError;
        _server.PartSelf += Server_PartSelf;
        _server.WhoisReceived += Server_WhoisReceived;
        _server.ServiceMessageReceived += Server_ServiceMessageReceived;
        _server.ServiceActionReceived += Server_ServiceActionReceived;
        _server.ServiceMessageSent += Server_ServiceMessageSent;
        _server.NoticeSent += Server_NoticeSent;
        _server.WentAway += Server_WentAway;
        _server.CameBack += Server_CameBack;
        _server.ServerChanged += Server_ServerChanged;
    }

    private string ConnectedStatus()
    {
        if (!_server.Connection.ConnectionData.Ssl)
            return ServerStrings.StatusConnected;

        return _server.CertificateAcceptance switch
        {
            CertificateAcceptance.VerificationOff => ServerStrings.StatusConnectedUnverified,
            CertificateAcceptance.Pinned => ServerStrings.StatusConnectedPinned,
            _ => ServerStrings.StatusConnectedSecure
        };
    }

    /// <summary>
    /// Says, in the server window, that the connection accepted its certificate because it
    /// matches the pin.
    /// </summary>
    private void SayIfConnectedThroughPin()
    {
        if (_server.CertificateAcceptance == CertificateAcceptance.Pinned)
            AddMessage(ServerStrings.ConnectedPinnedMessage.With(_server.Url, _server.Port));
    }

    /// <summary>
    /// Warns, in the status and in the server window, when the connection accepted its
    /// certificate only because the network's verify switch is off.
    /// </summary>
    private void WarnIfCertificateNotChecked()
    {
        if (_server.CertificateAcceptance != CertificateAcceptance.VerificationOff)
            return;

        IsConnectionStatusWarning = true;
        AddIncoming(new WarningMessageViewModel(DateTime.Now,
            ServerStrings.ConnectedUnverifiedWarning.With(_server.Url, _server.Port)));
    }

    // A panel's unread flag is one of the parts HasUnreadAnywhere is made of, so the aggregate
    // has to be re-announced whenever one of them changes, whenever the set of panels changes,
    // and whenever this server window's own flag changes (handled in OnPropertyChanged).
    private void UnreadSources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (INotifyPropertyChanged panel in e.OldItems ?? Array.Empty<object>())
            panel.PropertyChanged -= Panel_PropertyChanged;
        foreach (INotifyPropertyChanged panel in e.NewItems ?? Array.Empty<object>())
            panel.PropertyChanged += Panel_PropertyChanged;

        OnPropertyChanged(nameof(HasUnreadAnywhere));
    }

    private void Panel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HasUnread))
            OnPropertyChanged(nameof(HasUnreadAnywhere));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(HasUnread))
            OnPropertyChanged(nameof(HasUnreadAnywhere));
    }

    private void Server_WentAway(object? sender, EventArgs e) => IsAway = true;

    private void Server_CameBack(object? sender, EventArgs e) => IsAway = false;

    /// <summary>
    /// Returns whether an incoming away reply (RPL_AWAY) for <paramref name="nick"/> should be
    /// shown, suppressing a consecutive duplicate of the same away message. Records the message
    /// as shown when it returns true.
    /// </summary>
    public bool ShouldShowAwayReply(string nick, string message)
    {
        if (_shownAwayReplies.TryGetValue(nick, out string? last) && last == message)
            return false;

        _shownAwayReplies[nick] = message;
        return true;
    }

    private void Server_NoticeSent(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new OutgoingNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Server_ServiceMessageReceived(object? sender, UserMessageEventArgs e)
    {
        AddIncoming(new ChannelMessageViewModel(DateTime.Now, e.Message, e.User));
    }

    private void Server_ServiceMessageSent(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Message, _server.UserNick));
    }

    private void Server_ServiceActionReceived(object? sender, UserMessageEventArgs e)
    {
        AddIncoming(new ChannelActionViewModel(DateTime.Now, e.Message, e.User));
    }

    public ServerViewModel()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, "Foo"));
            ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Message", "Ortzinator"));
            ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "puts a donk on it", "Ortzinator"));
            ChatLines.Add(new IrcErrorViewModel(DateTime.Now, "Something bad happened", "14859"));
            ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, "A notice for you", "aUser"));
            Name = "irc.foo.com";
            Host = "irc.foo.com";
            NetworkName = "FooNet";
            ConnectionStatus = ServerStrings.StatusConnectedSecure;
        }
    }

    private void Server_PartSelf(object? sender, Common.PartEventArgs e)
    {
        string channelName = e.Channel.Name;

        // The channel list is the bound network's. A part comes after registration, which
        // binds the connection.
        OnUiThread(() =>
        {
            if (_boundNetwork is not { } network)
                return;

            // Membership of the network's channel list is the autojoin flag now, so leaving a
            // channel takes it off the list rather than leaving a chip that does nothing. While
            // the Networks dialog is open, the removal waits, and then finds the channel by name.
            IrcSettingsManager settings = IrcSettingsManager.Instance;
            settings.WhenWritable(() => settings.RemoveChannel(network, channelName));
        });
    }

    private void Server_NickError(object? sender, NickErrorEventArgs e)
    {
        if (_server.Connection.Registered || _server.Connection.HandleNickTaken) return;
        string newNick;
        switch (_nickRetryAttempt)
        {
            case 0:
                newNick = AlternateNick();
                DisplayNickTakenMessage(e.BadNick, newNick);
                _server.Connection.Sender.Register(newNick);
                _nickRetryAttempt = 1;
                break;
            case 1:
                newNick = AppSettings.Instance.ThirdNick;
                DisplayNickTakenMessage(e.BadNick, newNick);
                _server.Connection.Sender.Register(AppSettings.Instance.ThirdNick);
                _nickRetryAttempt = 2;
                break;
        }

        if (_nickRetryAttempt == 2 || _nickRetryFailed)
        {
            _nickRetryFailed = true;
            string nick = "MutinyIRC" + Random.Shared.Next(1000, 10000);
            _server.Connection.Sender.Register(nick);
        }
    }

    /// <summary>
    /// The nick to try when the first one is taken: the bound network's own alternate where it
    /// names one, and the app-wide second nick where it does not. Identity belongs to the
    /// network, so the network answers first. A connection with no network yet uses the second
    /// nick.
    /// </summary>
    /// <remarks>
    /// It runs on the socket thread. It reads one property of the bound network and does not
    /// go through the settings lists, which only the UI thread can read safely.
    /// </remarks>
    private string AlternateNick()
    {
        string? networkNick = _boundNetwork?.AlternateNick;
        return string.IsNullOrWhiteSpace(networkNick)
            ? AppSettings.Instance.SecondNick
            : networkNick;
    }

    private void DisplayNickTakenMessage(string nick, string newNick)
    {
        AddMessage(ServerStrings.NickTakenMessage.With(nick, newNick));
    }

    private void Server_ConnectCancelled(object? sender, EventArgs e)
    {
        ConnectionStatus = ServerStrings.StatusDisconnected;
        AddMessage(ServerStrings.Disconnected);
    }

    private void Server_ConnectionLost(object? sender, DisconnectEventArgs e)
    {
        ResetAwayState();
        // A reconnect is usually scheduled straight after this, and overwrites the status.
        ConnectionStatus = ServerStrings.StatusDisconnected;
        // Only SocketError has a socket error code. Every other reason needs its own wording,
        // or the socket wording says "0".
        AddMessage(e.Reason switch
        {
            DisconnectReason.SocketError =>
                ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)),
            DisconnectReason.RemoteHostClosedConnection => ServerStrings.ConnectionLostServerClosed,
            DisconnectReason.PingTimeout => ServerStrings.ConnectionLostPingTimeout,
            // DisconnectReason.Other, and any value that a newer FlamingIRC adds. Server reports
            // UserInitiated as Disconnected, not here.
            _ => ServerStrings.ConnectionLostUnknownMessage,
        });
    }

    // The Server owns the reconnect timer and backoff; we just render what it announces.
    private void Server_Reconnecting(object? sender, ReconnectingEventArgs e)
    {
        ConnectionStatus = ServerStrings.StatusReconnecting.With(e.Attempt);
        AddMessage(ServerStrings.AttemptingReconnect.With((int)e.Delay.TotalSeconds));
    }

    // The newest stop block, the only one whose Reconnect button works. Only the UI thread reads
    // or writes it.
    private EveryEntryPointRefusedViewModel? _reconnectableStop;

    // Every entry point refused its certificate, so the Server stopped and nothing reconnects
    // until the user clicks Reconnect.
    private void Server_EveryEntryPointRefused(object? sender, EventArgs e)
    {
        ConnectionStatus = ServerStrings.StatusCertificateRefused;
        var stop = new EveryEntryPointRefusedViewModel(DateTime.Now, ReconnectFromStop);
        OnUiThread(() =>
        {
            RetireStop();
            _reconnectableStop = stop;
        });
        AddIncoming(stop);
    }

    // The bound network has no entry points now, or the dialog deleted it, so the Server stopped
    // and nothing reconnects.
    private void Server_NoEntryPoints(object? sender, EventArgs e)
    {
        ConnectionStatus = ServerStrings.StatusDisconnected;
        AddIncoming(new ErrorMessageViewModel(DateTime.Now, ServerStrings.NoEntryPointsMessage));
    }

    /// <summary>Takes the Reconnect button off the newest stop block, if it still has one.</summary>
    private void RetireStop()
    {
        if (_reconnectableStop != null)
            _reconnectableStop.CanReconnect = false;
        _reconnectableStop = null;
    }

    /// <summary>Starts a new connect attempt from the first entry point of the network.</summary>
    private void ReconnectFromStop(EveryEntryPointRefusedViewModel stop)
    {
        if (stop != _reconnectableStop)
            return;

        RetireStop();
        _server.Reconnect();
    }

    private void Server_Disconnected(object? sender, EventArgs e)
    {
        ResetAwayState();
        ConnectionStatus = ServerStrings.StatusDisconnected;
        AddMessage(ServerStrings.Disconnected);
    }

    // Away status and the per-nick reply history are connection-scoped; a dropped connection
    // clears both so a reconnect starts fresh.
    private void ResetAwayState()
    {
        IsAway = false;
        _shownAwayReplies.Clear();
    }

    // /server named a host. The connection has no network for it yet, so its next
    // registration finds the network by host.
    private void Server_ServerChanged(object? sender, EventArgs e) =>
        OnUiThread(() => _boundNetwork = null);

    private void Server_Connecting(object? sender, CancelEventArgs e)
    {
        // The connection keeps its bound network. Only the session state goes. Clear it in the
        // UI queue, so that it comes after the writes of the previous registration and before
        // those of the next one.
        OnUiThread(() =>
        {
            _registered = false;
            _session++;
        });
        ConnectionStatus = ServerStrings.StatusConnecting;
        AddMessage(ServerStrings.ConnectingMessage.With(_server.Url, _server.Port));
    }

    private void Server_ErrorMessageRecieved(object? sender, ErrorMessageEventArgs e)
    {
        if (e.Code == ReplyCode.ERR_NOMOTD)
        {
            Debug.WriteLine("Message ignored: " + e.Code);
            return;
        }

        AddIncoming(new IrcErrorViewModel(DateTime.Now, e.Message, e.Code.ToString()));
    }

    /// <summary>
    /// Appends a connection-level server NOTICE (one with no sender nick, such as a
    /// pre-registration notice) as a plain informational line in the server window.
    /// </summary>
    public void AddServerNotice(string message) => AddMessage(message);

    /// <summary>
    /// Asks the user to confirm that a changed pinned certificate replaces the pin. The view sets
    /// it to show a dialog. The default says no, so a pin never changes without a confirm.
    /// </summary>
    public Func<CertificateRefusalViewModel, Task<bool>> ConfirmPinReplacement { get; set; } =
        _ => Task.FromResult(false);

    // The newest refusal, the only one whose button works. Only the UI thread reads or writes it.
    private CertificateRefusalViewModel? _trustableRefusal;

    private void Server_ConnectFailed(object? sender, ConnectFailedEventArgs e)
    {
        if (e.RejectedCertificate is { } certificate)
        {
            AddRefusal(certificate);
            return;
        }

        // Only SocketError has a socket error code. Every other reason needs its own wording,
        // or the socket wording says "0".
        AddMessage(e.Reason switch
        {
            ConnectError.SocketError =>
                ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)),
            ConnectError.ConnectionRefused =>
                ServerStrings.ConnectionFailedMessage.With(SocketErrorStrings.ConnectionRefused),
            ConnectError.CertificateRejected => ServerStrings.CertificateRejectedMessage,
            ConnectError.AuthenticationFailed => ServerStrings.TlsHandshakeFailedMessage,
            // ConnectError.Other, and any value that a newer FlamingIRC adds.
            _ => ServerStrings.ConnectionFailedUnknownMessage,
        });
    }

    /// <summary>
    /// Shows the refusal of <paramref name="certificate"/> with its Trust button, and takes the
    /// button off the older refusals.
    /// </summary>
    private void AddRefusal(RejectedCertificate certificate)
    {
        // Server raises ConnectFailed before it moves on, so Connection is the refused entry point.
        ConnectionArgs entryPoint = _server.Connection.ConnectionData;

        // The same lookup that the certificate callback reads, so the refusal and the decision
        // see the same pin.
        string? pin = (_server.CertificateTrustLookup?.Invoke(entryPoint.Hostname, entryPoint.Port)
                       ?? CertificateTrust.Default).PinnedFingerprint;

        var refusal = new CertificateRefusalViewModel(DateTime.Now,
            CertificateRefusal.Describe(certificate, entryPoint.Hostname, entryPoint.Port, pin),
            entryPoint.Hostname, entryPoint.Port,
            certificate.GetFingerprint(HashAlgorithmName.SHA256), pin, TrustCertificateAsync);

        OnUiThread(() =>
        {
            RetireRefusal();
            _trustableRefusal = refusal;
            refusal.EntryPointRemoved = !BoundNetworkLists(refusal.Host, refusal.Port);
        });
        AddIncoming(refusal);
    }

    /// <summary>Takes the Trust button off the newest refusal, if it still has one.</summary>
    private void RetireRefusal()
    {
        if (_trustableRefusal != null)
            _trustableRefusal.CanTrust = false;
        _trustableRefusal = null;
    }

    // Server events come on socket threads. The refusal and stop state belongs to the UI thread,
    // because the buttons bind to it. Only the UI thread writes the network settings, because
    // the Networks dialog and the sidebar read and replace them there.
    private static void OnUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(action);
    }

    /// <summary>
    /// Pins the refused certificate on the refused entry point, then dials that entry point at
    /// once. A pin that changes needs a confirm first.
    /// </summary>
    private async Task TrustCertificateAsync(CertificateRefusalViewModel refusal)
    {
        if (refusal != _trustableRefusal)
            return;

        if (refusal.ReplacesPin && !await ConfirmPinReplacement(refusal))
            return;

        // The connection may have registered, or refused again, while the dialog was open.
        if (refusal != _trustableRefusal)
            return;

        // A save may have removed the entry point from the bound network. Then a pin would go
        // to another network, or mint one.
        if (!BoundNetworkLists(refusal.Host, refusal.Port))
        {
            refusal.EntryPointRemoved = true;
            return;
        }

        // A host that no saved network lists gets a network of its own to hold the pin. A refusal
        // is always TLS, so the minted entry point is too.
        IrcSettingsManager settings = IrcSettingsManager.Instance;
        if (!settings.PinCertificate(refusal.Host, refusal.Port, refusal.Fingerprint)
            && settings.MintPinnedNetwork(refusal.Host, refusal.Port, refusal.Fingerprint) == null)
        {
            AddIncoming(new ErrorMessageViewModel(DateTime.Now,
                ServerStrings.CertificatePinHostOnOtherPort.With(refusal.Host, refusal.Port)));
            return;
        }

        RetireRefusal();
        _server.DialEntryPoint(refusal.Host, refusal.Port);
    }

    private void Server_Registered(object? sender, EventArgs e)
    {
        // Read the entry point now. The connection can move to another entry point before the
        // UI thread runs the writes.
        var entryPoint = new ServerSettings(_server.Url, _server.Port, _server.Connection.ConnectionData.Ssl);
        string reportedName = _server.Connection.ServerProperties["Network"];

        // The nick retry state belongs to the socket thread, where the next 433 arrives.
        bool usedRandomNick = _nickRetryFailed;
        _nickRetryAttempt = 0;
        _nickRetryFailed = false;

        OnUiThread(() =>
        {
            RetireRefusal();
            RetireStop();
            DoRegister(entryPoint, reportedName, usedRandomNick);
        });
    }

    protected override void OnExecute(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return;

        CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(_server, commandLine));
        if (result != null && result.Result == Result.Fail)
        {
            ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
        }
    }

    private void AddMessage(string message)
    {
        AddIncoming(new ChatItemViewModel(DateTime.Now, message));
    }

    /// <summary>
    /// Shows that the connection registered, and files it under the network it belongs to. Runs
    /// on every registration, so it also picks up a network the server has renamed and an entry
    /// point we reached for the first time.
    /// </summary>
    /// <param name="entryPoint">The entry point that registered.</param>
    /// <param name="reportedName">
    /// The network name that the server reported before it registered, or empty.
    /// </param>
    /// <param name="usedRandomNick">Whether every nick was taken, so the server has a random one.</param>
    /// <remarks>
    /// Call it on the UI thread. While the Networks dialog is open, the settings writes wait. A
    /// bound connection joins the channels of its bound network at once. A connection with no
    /// network yet uses the second nick as its alternate nick, and joins its channels when its
    /// writes run.
    /// </remarks>
    private void DoRegister(ServerSettings entryPoint, string reportedName, bool usedRandomNick)
    {
        _registered = true;
        ConnectionStatus = ConnectedStatus();
        WarnIfCertificateNotChecked();
        SayIfConnectedThroughPin();

        if (usedRandomNick)
            AddMessage(ServerStrings.RandomNickMessage);

        NetworkSettings? joined = _boundNetwork;
        if (joined != null)
        {
            ShowNetwork(joined);
            JoinAutoJoinChannels(joined);
        }

        int session = _session;
        IrcSettingsManager.Instance.WhenWritable(
            () => FileUnderNetwork(entryPoint, reportedName, session, joined));
    }

    /// <summary>
    /// Writes the network settings for a registration: it finds or settles the network, binds
    /// the connection to it, and marks it for startup. It joins the channels of the network if
    /// the registration has not joined them and the session is still the same.
    /// </summary>
    /// <param name="entryPoint">The entry point that registered.</param>
    /// <param name="reportedName">The network name that the server reported, or empty.</param>
    /// <param name="session">The session of the registration.</param>
    /// <param name="joined">The network whose channels the registration joined, or null.</param>
    /// <remarks>
    /// It can run after the Networks dialog closes, so it reads the binding then, not at the
    /// registration.
    /// </remarks>
    private void FileUnderNetwork(
        ServerSettings entryPoint, string reportedName, int session, NetworkSettings? joined)
    {
        NetworkSettings network;
        if (_boundNetwork is { } bound)
        {
            // The dialog deleted the network. Registration does not write it back.
            if (!IsSaved(bound))
                return;

            // The connection keeps the network it was dialed for. It does not look up its host
            // again, so an edit or a move of its entry point does not change the network, and
            // the entry point does not go back into the settings.
            network = reportedName == string.Empty ? bound : SettleByReportedName(bound, reportedName);
        }
        else
        {
            network = ResolveNetwork(entryPoint.Url, reportedName);

            // AddServer keys on the URL and ignores a host the network already lists, so this
            // both donates a new entry point and leaves a known one alone.
            network.AddServer(entryPoint);
        }

        _boundNetwork = network;
        AdoptNetwork(network);
        if (network != joined && session == _session && _registered)
            JoinAutoJoinChannels(network);
    }

    // The bound network: the network that this connection was dialed for. It is null until the
    // connection has a network: a connection that /server made finds its network by host at its
    // first registration. Only the UI thread writes it. The socket thread reads it for the
    // alternate nick, thus it is volatile.
    private volatile NetworkSettings? _boundNetwork;

    // Whether the current session has registered. A new connect attempt clears it. Only the UI
    // thread reads or writes it.
    private bool _registered;

    // The number of the current session. Each connect attempt adds one, so that a write that
    // waited for the Networks dialog does not join channels for an old session. Only the UI
    // thread reads or writes it.
    private int _session;

    /// <summary>
    /// The network that this connection was dialed for (its bound network), or null when the
    /// connection has no network yet. Read it on the UI thread.
    /// </summary>
    internal NetworkSettings? BoundNetwork => _boundNetwork;

    /// <summary>
    /// Binds this connection to <paramref name="network"/>, the network that it is dialed for.
    /// Registration then uses that network, and does not find the network by host.
    /// </summary>
    /// <remarks>Call it on the UI thread, before the connection dials.</remarks>
    internal void BindNetwork(NetworkSettings network) => _boundNetwork = network;

    /// <summary>
    /// Gives the connection the entry points of its bound network, after a Networks dialog save.
    /// The session stays up, and the new list applies at the next dial. When the save changed or
    /// removed the entry point that the connection is on, the server panel shows one line.
    /// </summary>
    /// <remarks>
    /// A network that the dialog deleted has no entry points for the connection. With no entry
    /// points, the connection stops at its next dial. The Trust button of the newest refusal is
    /// disabled when the bound network does not list the refused entry point now. It reads the
    /// settings. Call it on the UI thread.
    /// </remarks>
    internal void FollowSavedEntryPoints()
    {
        if (_boundNetwork is not { } network)
            return;

        IReadOnlyList<ConnectionArgs> entryPoints = IsSaved(network)
            ? network.ToConnectionCandidates()
            : Array.Empty<ConnectionArgs>();
        if (_server.ReplaceEntryPoints(entryPoints))
            AddMessage(ServerStrings.EntryPointChangedMessage);

        if (_trustableRefusal is { } refusal)
            refusal.EntryPointRemoved = !BoundNetworkLists(refusal.Host, refusal.Port);
    }

    /// <summary>Whether <paramref name="network"/> is in the saved settings.</summary>
    private static bool IsSaved(NetworkSettings network) =>
        IrcSettingsManager.Instance.Networks.Contains(network);

    /// <summary>
    /// Whether the bound network is saved and lists the entry point <paramref name="host"/>:
    /// <paramref name="port"/>. A connection with no network yet has no such limit, thus it is
    /// true for it.
    /// </summary>
    /// <remarks>It reads the settings. Call it on the UI thread.</remarks>
    private bool BoundNetworkLists(string host, int port) =>
        _boundNetwork is not { } bound
        || (IsSaved(bound) && bound.Servers.Any(entryPoint =>
                entryPoint.Port == port
                && entryPoint.Url.Equals(host, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Gives the certificate trust settings for the entry point <paramref name="host"/>:
    /// <paramref name="port"/> of this connection. A bound connection takes the verify switch
    /// from its bound network, and the pin from that entry point in its bound network. A
    /// connection with no network yet finds the network by host.
    /// </summary>
    /// <remarks>It reads the settings. Call it on the UI thread.</remarks>
    internal CertificateTrust GetCertificateTrust(string host, int port) =>
        _boundNetwork is { } bound
            ? IrcSettingsManager.GetCertificateTrust(bound, host, port)
            : IrcSettingsManager.Instance.GetCertificateTrust(host, port);

    // The network that registration named after the host, because no saved network listed the
    // host and the server had not reported a name yet. The first reported name can still move
    // the host into a saved network of that name.
    private NetworkSettings? _hostNamedNetwork;

    /// <summary>
    /// Marks <paramref name="network"/> for startup and shows its name in the server window.
    /// </summary>
    private void AdoptNetwork(NetworkSettings network)
    {
        // Connecting to a network is what marks it worth dialling at startup. The flag sits on
        // the network, so reaching it through any of its hosts sets it once.
        IrcSettingsManager.Instance.MarkAutoConnect(network);
        ShowNetwork(network);
    }

    /// <summary>Shows the name of <paramref name="network"/> in the server window.</summary>
    private void ShowNetwork(NetworkSettings network)
    {
        Name = ServerStrings.ServerFormTitleBar.With(
            _server.UserNick,
            network.Name,
            _server.Url,
            _server.Port);
        NetworkName = network.Name;
    }

    private void JoinAutoJoinChannels(NetworkSettings network)
    {
        foreach (ChannelSettings channel in network.Channels)
        {
            if (channel.AutoJoin)
                _server.JoinChannel(channel.Name, channel.Key ?? string.Empty);
        }
    }

    /// <summary>
    /// Finds the saved network that the entry point <paramref name="host"/> belongs to, minting
    /// one if it belongs to none. <paramref name="reportedName"/> is the network name that the
    /// server reported, or empty.
    /// </summary>
    /// <remarks>
    /// The entry point identifies the network, so a saved host wins outright. Otherwise the name
    /// the server reports is the only link we have, and a host that reports a known name joins
    /// that network instead of standing up a duplicate. A server that reports no name leaves the
    /// host as the sole thing to name the network after.
    ///
    /// Most servers report the name in 005, after the 001 that registers the connection.
    /// <see cref="Server_NetworkNameReported"/> then does the merge and the rename.
    /// </remarks>
    private NetworkSettings ResolveNetwork(string host, string reportedName)
    {
        IrcSettingsManager settings = IrcSettingsManager.Instance;

        NetworkSettings? saved = settings.GetNetwork(host);
        NetworkSettings network = saved
            ?? settings.GetOrAddNetwork(reportedName == string.Empty ? host : reportedName);

        _hostNamedNetwork = saved == null && reportedName == string.Empty ? network : null;

        return reportedName == string.Empty ? network : SettleByReportedName(network, reportedName);
    }

    /// <summary>
    /// Applies the network name that the server reported to <paramref name="network"/>.
    /// </summary>
    /// <remarks>
    /// A network that the pin action minted, or that registration named after the host, is the
    /// exception. Only Mutiny made it, so the reported name can still move its host into a saved
    /// network of that name.
    /// </remarks>
    /// <returns>The network that the entry point now belongs to.</returns>
    private NetworkSettings SettleByReportedName(NetworkSettings network, string reportedName)
    {
        if (network.MintedByPin || network == _hostNamedNetwork)
            network = IrcSettingsManager.Instance.SettleMintedNetwork(network, reportedName);
        _hostNamedNetwork = null;

        // The server owns the name and may change it on any connect.
        IrcSettingsManager.Instance.RenameNetwork(network, reportedName);
        return network;
    }

    // Servers send NETWORK= in 005, after the 001 that registered the connection. A name that
    // comes before registration is for DoRegister to read.
    private void Server_NetworkNameReported(object? sender, Common.DataEventArgs<string> e)
    {
        string reportedName = e.Data;

        // The UI queue keeps the order of the events, so the registration before this 005 has
        // run when this runs. The held writes keep their order too, so the writes of that
        // registration have run when the writes of this name run.
        OnUiThread(() =>
        {
            if (!_registered)
                return;

            int session = _session;
            IrcSettingsManager.Instance.WhenWritable(() =>
            {
                // The dialog deleted the network. Registration does not write it back.
                if (_boundNetwork is not { } bound || !IsSaved(bound))
                    return;

                NetworkSettings settled = SettleByReportedName(bound, reportedName);
                AdoptNetwork(settled);
                if (settled != bound && session == _session && _registered)
                    JoinAutoJoinChannels(settled);
                _boundNetwork = settled;
            });
        });
    }

    private void Server_WhoisReceived(object? sender, Common.DataEventArgs<WhoisInfo> e)
    {
        WhoisInfo info = e.Data;
        string nick = info.User?.Nick ?? "?";

        AddMessage($"{nick} is {info.User?.Nick}!{info.User?.UserName}@{info.User?.HostMask} * {info.RealName}");

        if (!string.IsNullOrEmpty(info.Server))
            AddMessage($"{nick} server: {info.Server} [{info.ServerDescription}]");

        if (info.IdleTime > 0)
            AddMessage($"{nick} idle: {FormatIdleTime(info.IdleTime)}");

        if (info.Operator)
            AddMessage($"{nick} is an IRC operator");

        string[]? channels = info.GetChannels();
        if (channels != null && channels.Length > 0)
            AddMessage($"{nick} channels: {string.Join(" ", channels)}");

        AddMessage($"End of WHOIS for {nick}");
    }

    private static string FormatIdleTime(long seconds)
    {
        var sb = new StringBuilder();
        long h = seconds / 3600;
        long m = (seconds % 3600) / 60;
        long s = seconds % 60;
        if (h > 0) sb.Append($"{h}h ");
        if (m > 0) sb.Append($"{m}m ");
        sb.Append($"{s}s");
        return sb.ToString().TrimEnd();
    }

    public override void Close()
    {
        base.Close();
        _server?.Disconnect(RandomMessages.Instance.GetMessage("quit") ?? "MutinyIRC");
    }

    public override void Dispose()
    {
        if (_server == null)
            return;

        Channels.CollectionChanged -= UnreadSources_CollectionChanged;
        PrivateMessages.CollectionChanged -= UnreadSources_CollectionChanged;
        foreach (ChannelViewModel channel in Channels)
            channel.PropertyChanged -= Panel_PropertyChanged;
        foreach (PrivateMessageViewModel pm in PrivateMessages)
            pm.PropertyChanged -= Panel_PropertyChanged;

        // Detach every handler wired in the constructor so the Server (which outlives this VM)
        // doesn't keep it alive. Keep this list in sync with the constructor's subscriptions.
        _server.Registered -= Server_Registered;
        _server.NetworkNameReported -= Server_NetworkNameReported;
        _server.ConnectFailed -= Server_ConnectFailed;
        _server.Reconnecting -= Server_Reconnecting;
        _server.EveryEntryPointRefused -= Server_EveryEntryPointRefused;
        _server.NoEntryPoints -= Server_NoEntryPoints;
        _server.ErrorMessageRecieved -= Server_ErrorMessageRecieved;
        _server.Connecting -= Server_Connecting;
        _server.Disconnected -= Server_Disconnected;
        _server.ConnectionLost -= Server_ConnectionLost;
        _server.ConnectCancelled -= Server_ConnectCancelled;
        _server.NickError -= Server_NickError;
        _server.PartSelf -= Server_PartSelf;
        _server.WhoisReceived -= Server_WhoisReceived;
        _server.ServiceMessageReceived -= Server_ServiceMessageReceived;
        _server.ServiceActionReceived -= Server_ServiceActionReceived;
        _server.ServiceMessageSent -= Server_ServiceMessageSent;
        _server.NoticeSent -= Server_NoticeSent;
        _server.WentAway -= Server_WentAway;
        _server.CameBack -= Server_CameBack;
        _server.ServerChanged -= Server_ServerChanged;
    }
}
