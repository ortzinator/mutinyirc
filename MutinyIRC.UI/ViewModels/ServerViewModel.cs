using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using global::Avalonia.Controls;

namespace MutinyIRC.UI.ViewModels;

using System;
using FlamingIRC;
using Common;
using PluginFramework;
using Resources;

public class ServerViewModel : IrcViewModel
{
    private int nickRetryAttempt;
    private bool nickRetryFailed;
    private readonly Server server = null!;
    private readonly PluginManager _pluginManager = null!;

    public MTObservableCollection<ChannelViewModel> Channels { get; } = new MTObservableCollection<ChannelViewModel>();
    internal Server? ServerInstance => server;

    public ServerViewModel(Server newServer, PluginManager pluginManager)
    {
        if (global::Avalonia.Controls.Design.IsDesignMode)
            return;

        _pluginManager = pluginManager;
        server = newServer;
        Name = server.Url;
        server.Registered += Server_Registered;
        server.ConnectFailed += Server_ConnectFailed;
        server.PrivateNotice += Server_PrivateNotice;
        server.ErrorMessageRecieved += Server_ErrorMessageRecieved;
        server.Connecting += Server_Connecting;
        server.Disconnected += Server_Disconnected;
        server.ConnectionLost += Server_ConnectionLost;
        server.ConnectCancelled += Server_ConnectCancelled;
        server.NickError += Server_NickError;
        server.PartSelf += Server_PartSelf;
        server.WhoisReceived += Server_WhoisReceived;
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
        var nwSettings = IrcSettingsManager.Instance.GetNetwork(server);
        var chan = nwSettings?.GetChannel(e.Channel.Name);
        if (chan != null)
            chan.AutoJoin = false;
    }

    private void Server_NickError(object? sender, NickErrorEventArgs e)
    {
        if (server.Connection.Registered || server.Connection.HandleNickTaken) return;
        string newNick;
        switch (nickRetryAttempt)
        {
            case 0:
                newNick = AppSettings.Instance.SecondNick;
                DisplayNickTakenMessage(e.BadNick, newNick);
                server.Connection.Sender.Register(newNick);
                nickRetryAttempt = 1;
                break;
            case 1:
                newNick = AppSettings.Instance.ThirdNick;
                DisplayNickTakenMessage(e.BadNick, newNick);
                server.Connection.Sender.Register(AppSettings.Instance.ThirdNick);
                nickRetryAttempt = 2;
                break;
        }

        if (nickRetryAttempt == 2 || nickRetryFailed)
        {
            nickRetryFailed = true;
            string nick = "MutinyIRC" + Random.Shared.Next(1000, 10000);
            server.Connection.Sender.Register(nick);
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
        AddMessage(ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));

        if (e.Reason != DisconnectReason.UserInitiated)
        {
            AddMessage(ServerStrings.AttemptingReconnect);
            ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
        }
    }

    private void Server_Disconnected(object? sender, EventArgs e)
    {
        AddMessage(ServerStrings.Disconnected);
    }

    private void Server_Connecting(object? sender, CancelEventArgs e)
    {
        AddMessage(ServerStrings.ConnectingMessage.With(server.Url, server.Port));
    }

    private void Server_ErrorMessageRecieved(object? sender, ErrorMessageEventArgs e)
    {
        if (e.Code == ReplyCode.ERR_NOMOTD)
        {
            Debug.WriteLine("Message ignored: " + e.Code);
            return;
        }

        ChatLines.Add(new IrcErrorViewModel(DateTime.Now, e.Message, e.Code.ToString()));
    }

    private void Server_PrivateNotice(object? sender, UserMessageEventArgs e)
    {
        // Pre-registration server NOTICEs come from User.Empty (no nick).
        // Display them as plain informational messages, not as user-to-user notices.
        if (string.IsNullOrEmpty(e.User.Nick))
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, e.Message));
        else
            ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Server_ConnectFailed(object? sender, ConnectFailedEventArgs e)
    {
        AddMessage(ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));
        ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
    }

    private void Server_Registered(object? sender, EventArgs e)
    {
        DoRegister();
    }

    protected override void OnExecute(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return;

        CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(server, commandLine));
        if (result != null && result.Result == Result.Fail)
        {
            ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
        }
    }

    private void AddMessage(string message)
    {
        ChatLines.Add(new ChatItemViewModel(DateTime.Now, message));
    }

    private void DoRegister()
    {
        string network = server.Connection.ServerProperties["Network"];
        NetworkSettings? networkSettings = IrcSettingsManager.Instance.GetNetwork(server);

        if (networkSettings == null)
        {
            NetworkSettings? tempNet;
            if (network == string.Empty)
            {
                tempNet = IrcSettingsManager.Instance.AddNetwork(server.Url);
                network = "Network";
            }
            else
            {
                tempNet = IrcSettingsManager.Instance.AddNetwork(network);
            }

            tempNet?.AddServer(new ServerSettings(server.Url, "Random", server.Port.ToString(),
                    server.Connection.ConnectionData.Ssl)
            { AutoConnect = true });
        }
        else
        {
            if (network == string.Empty)
                network = networkSettings.Name;
            else
                networkSettings.Name = network;

            ServerSettings? nServer = networkSettings.GetServer(server.Url);
            if (nServer == null)
            {
                networkSettings.AddServer(new ServerSettings(server.Url, "Random", server.Port.ToString(),
                    server.Connection.ConnectionData.Ssl)
                { AutoConnect = true });
            }
        }

        Name = ServerStrings.ServerFormTitleBar.With(
                server.UserNick,
                network,
                server.Url,
                server.Port);

        if (nickRetryFailed)
            AddMessage(ServerStrings.RandomNickMessage);

        nickRetryAttempt = 0;
        nickRetryFailed = false;

        if (networkSettings == null || networkSettings.Channels == null) return;
        foreach (ChannelSettings channel in networkSettings.Channels)
        {
            if (channel.AutoJoin)
                server.JoinChannel(channel.Name, channel.Key ?? string.Empty);
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
        server?.Disconnect(RandomMessages.Instance.GetMessage("quit") ?? "MutinyIRC");
    }

    public override void Dispose()
    {
        if (server != null)
            server.WhoisReceived -= Server_WhoisReceived;
    }
}
