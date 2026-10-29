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
        _server.ConnectFailed += Server_ConnectFailed;
        _server.Reconnecting += Server_Reconnecting;
        _server.EveryEntryPointRefused += Server_EveryEntryPointRefused;
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
    }

    private string ConnectedStatus()
    {
        if (!_server.Connection.ConnectionData.Ssl)
            return ServerStrings.StatusConnected;

        return _server.CertificateAcceptance switch
        {
            CertificateAcceptance.VerificationOff => ServerStrings.StatusConnectedUnverified,
            CertificateAcceptance.Pinned => ServerStrings.StatusConnectedPinned,
            _ => ServerStrings.StatusConnectedSecure,
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
        var nwSettings = IrcSettingsManager.Instance.GetNetwork(_server);
        var chan = nwSettings?.GetChannel(e.Channel.Name);

        // Membership of the network's channel list is the autojoin flag now, so leaving a
        // channel takes it off the list rather than leaving a chip that does nothing.
        if (chan != null)
            nwSettings!.RemoveChannel(chan);
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
    /// The nick to try when the first one is taken: the network's own alternate where it names
    /// one, and the app-wide second nick where it does not. Identity belongs to the network, so
    /// the network answers first.
    /// </summary>
    private string AlternateNick()
    {
        string? networkNick = IrcSettingsManager.Instance.GetNetwork(_server)?.AlternateNick;
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

    // Every entry point refused its certificate, so the Server stopped and nothing reconnects.
    private void Server_EveryEntryPointRefused(object? sender, EventArgs e)
    {
        ConnectionStatus = ServerStrings.StatusCertificateRefused;
        AddIncoming(new ErrorMessageViewModel(DateTime.Now, ServerStrings.EveryEntryPointRefusedMessage));
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

    private void Server_Connecting(object? sender, CancelEventArgs e)
    {
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

    // The refusal state belongs to the UI thread, because the Trust button binds to it. Server
    // events come on socket threads.
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

        if (!IrcSettingsManager.Instance.PinCertificate(refusal.Host, refusal.Port, refusal.Fingerprint))
        {
            AddIncoming(new ErrorMessageViewModel(DateTime.Now,
                ServerStrings.CertificatePinNeedsSavedNetwork.With(refusal.Host, refusal.Port)));
            return;
        }

        RetireRefusal();
        _server.DialEntryPoint(refusal.Host, refusal.Port);
    }

    private void Server_Registered(object? sender, EventArgs e)
    {
        OnUiThread(RetireRefusal);
        DoRegister();
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
    /// Files this connection under the network it belongs to, then joins that network's autojoin
    /// channels. Runs on every registration, so it also picks up a network the server has renamed
    /// and an entry point we reached for the first time.
    /// </summary>
    private void DoRegister()
    {
        NetworkSettings network = ResolveNetwork();

        // AddServer keys on the URL and ignores a host the network already lists, so this both
        // donates a new entry point and leaves a known one alone.
        network.AddServer(new ServerSettings(_server.Url, _server.Port,
            _server.Connection.ConnectionData.Ssl));

        // Connecting to a network is what marks it worth dialling at startup. The flag sits on
        // the network, so reaching it through any of its hosts sets it once.
        network.AutoConnect = true;

        Name = ServerStrings.ServerFormTitleBar.With(
            _server.UserNick,
            network.Name,
            _server.Url,
            _server.Port);
        NetworkName = network.Name;
        ConnectionStatus = ConnectedStatus();
        WarnIfCertificateNotChecked();
        SayIfConnectedThroughPin();

        if (_nickRetryFailed)
            AddMessage(ServerStrings.RandomNickMessage);

        _nickRetryAttempt = 0;
        _nickRetryFailed = false;

        foreach (ChannelSettings channel in network.Channels)
        {
            if (channel.AutoJoin)
                _server.JoinChannel(channel.Name, channel.Key ?? string.Empty);
        }
    }

    /// <summary>
    /// Finds the saved network this connection belongs to, minting one if it belongs to none.
    /// </summary>
    /// <remarks>
    /// The entry point identifies the network, so a saved host wins outright. Otherwise the name
    /// the server reports is the only link we have, and a host that reports a known name joins
    /// that network instead of standing up a duplicate. A server that reports no name leaves the
    /// host as the sole thing to name the network after.
    /// </remarks>
    private NetworkSettings ResolveNetwork()
    {
        string reportedName = _server.Connection.ServerProperties["Network"];

        NetworkSettings network = IrcSettingsManager.Instance.GetNetwork(_server)
            ?? IrcSettingsManager.Instance.GetOrAddNetwork(
                   reportedName == string.Empty ? _server.Url : reportedName);

        // The server owns the name and may change it on any connect.
        if (reportedName != string.Empty)
            network.Name = reportedName;

        return network;
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
        _server.ConnectFailed -= Server_ConnectFailed;
        _server.Reconnecting -= Server_Reconnecting;
        _server.EveryEntryPointRefused -= Server_EveryEntryPointRefused;
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
    }
}
