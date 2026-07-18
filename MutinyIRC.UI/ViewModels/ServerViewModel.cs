using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using global::Avalonia.Controls;

using System;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

public class ServerViewModel : IrcViewModel
{
    private int _nickRetryAttempt;
    private bool _nickRetryFailed;
    private readonly Server _server = null!;
    private readonly PluginManager _pluginManager = null!;

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

    // The away message last shown for each nick, so messaging an away user repeatedly doesn't
    // re-print their away text. A changed message shows again; cleared when the connection drops.
    private readonly Dictionary<string, string> _shownAwayReplies = new(StringComparer.OrdinalIgnoreCase);

    public ServerViewModel(Server newServer, PluginManager pluginManager)
    {
        if (global::Avalonia.Controls.Design.IsDesignMode)
            return;

        _pluginManager = pluginManager;
        _server = newServer;
        Name = _server.Url;
        _server.Registered += Server_Registered;
        _server.ConnectFailed += Server_ConnectFailed;
        _server.Reconnecting += Server_Reconnecting;
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
        if (global::Avalonia.Controls.Design.IsDesignMode)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, "Foo"));
            ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Message", "Ortzinator"));
            ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "puts a donk on it", "Ortzinator"));
            ChatLines.Add(new IrcErrorViewModel(DateTime.Now, "Something bad happened", "14859"));
            ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, "A notice for you", "aUser"));
            Name = "irc.foo.com";
        }
    }

    private void Server_PartSelf(object? sender, PartEventArgs e)
    {
        var nwSettings = IrcSettingsManager.Instance.GetNetwork(_server);
        var chan = nwSettings?.GetChannel(e.Channel.Name);
        if (chan != null)
            chan.AutoJoin = false;
    }

    private void Server_NickError(object? sender, NickErrorEventArgs e)
    {
        if (_server.Connection.Registered || _server.Connection.HandleNickTaken) return;
        string newNick;
        switch (_nickRetryAttempt)
        {
            case 0:
                newNick = AppSettings.Instance.SecondNick;
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

    private void DisplayNickTakenMessage(string nick, string newNick)
    {
        AddMessage(ServerStrings.NickTakenMessage.With(nick, newNick));
    }

    private void Server_ConnectCancelled(object? sender, EventArgs e)
    {
        AddMessage(ServerStrings.Disconnected);
    }

    private void Server_ConnectionLost(object? sender, DisconnectEventArgs e)
    {
        ResetAwayState();
        AddMessage(ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));
    }

    // The Server owns the reconnect timer and backoff; we just render the announced delay.
    private void Server_Reconnecting(object? sender, Common.DataEventArgs<TimeSpan> e)
    {
        AddMessage(ServerStrings.AttemptingReconnect.With((int)e.Data.TotalSeconds));
    }

    private void Server_Disconnected(object? sender, EventArgs e)
    {
        ResetAwayState();
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

    private void Server_ConnectFailed(object? sender, ConnectFailedEventArgs e)
    {
        AddMessage(ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));
    }

    private void Server_Registered(object? sender, EventArgs e)
    {
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

    private void DoRegister()
    {
        string network = _server.Connection.ServerProperties["Network"];
        NetworkSettings? networkSettings = IrcSettingsManager.Instance.GetNetwork(_server);

        if (networkSettings == null)
        {
            NetworkSettings? tempNet;
            if (network == string.Empty)
            {
                tempNet = IrcSettingsManager.Instance.AddNetwork(_server.Url);
                network = "Network";
            }
            else
            {
                tempNet = IrcSettingsManager.Instance.AddNetwork(network);
            }

            tempNet?.AddServer(new ServerSettings(_server.Url, "Random", _server.Port.ToString(),
                    _server.Connection.ConnectionData.Ssl)
            { AutoConnect = true });
        }
        else
        {
            if (network == string.Empty)
                network = networkSettings.Name;
            else
                networkSettings.Name = network;

            ServerSettings? nServer = networkSettings.GetServer(_server.Url);
            if (nServer == null)
            {
                networkSettings.AddServer(new ServerSettings(_server.Url, "Random", _server.Port.ToString(),
                    _server.Connection.ConnectionData.Ssl)
                { AutoConnect = true });
            }
        }

        Name = ServerStrings.ServerFormTitleBar.With(
                _server.UserNick,
                network,
                _server.Url,
                _server.Port);

        if (_nickRetryFailed)
            AddMessage(ServerStrings.RandomNickMessage);

        _nickRetryAttempt = 0;
        _nickRetryFailed = false;

        if (networkSettings == null || networkSettings.Channels == null) return;
        foreach (ChannelSettings channel in networkSettings.Channels)
        {
            if (channel.AutoJoin)
                _server.JoinChannel(channel.Name, channel.Key ?? string.Empty);
        }
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

        // Detach every handler wired in the constructor so the Server (which outlives this VM)
        // doesn't keep it alive. Keep this list in sync with the constructor's subscriptions.
        _server.Registered -= Server_Registered;
        _server.ConnectFailed -= Server_ConnectFailed;
        _server.Reconnecting -= Server_Reconnecting;
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
