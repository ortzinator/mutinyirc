
using System;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

public class PrivateMessageViewModel : IrcViewModel
{
    private readonly PrivateMessageSession _session = null!;
    private readonly PluginManager _pluginManager = null!;

    public PrivateMessageSession Session => _session;
    public override Server? OwningServer => _session.Server;

    private string _otherNick = string.Empty;
    public string OtherNick
    {
        get => _otherNick;
        protected set => SetProperty(ref _otherNick, value);
    }

    /// <summary>The only completable name in a private chat is the person you're talking to.</summary>
    public override System.Collections.Generic.IReadOnlyList<string> CompletionCandidates
        => new[] { OtherNick };

    private bool _isServerConnected;
    public bool IsServerConnected
    {
        get => _isServerConnected;
        protected set => SetProperty(ref _isServerConnected, value);
    }

    protected PrivateMessageViewModel() { }

    public PrivateMessageViewModel(PrivateMessageSession session, PluginManager pluginManager)
    {
        if (global::Avalonia.Controls.Design.IsDesignMode)
            return;

        _session = session;
        _pluginManager = pluginManager;
        OtherNick = session.User.Nick;
        Name = session.User.Nick;
        _isServerConnected = session.Server.IsConnected;

        _session.MessageReceived += Session_MessageReceived;
        _session.MessageSent += Session_MessageSent;
        _session.ActionReceived += Session_ActionReceived;
        _session.ActionSent += Session_ActionSent;
        _session.NoticeSent += Session_NoticeSent;

        _session.Server.OnNick += Server_OnNick;
        _session.Server.Disconnected += Server_Disconnected;
        _session.Server.Connected += Server_Connected;
        _session.Server.ConnectionLost += Server_ConnectionLost;
    }

    private void Session_MessageReceived(object? sender, Common.DataEventArgs<string> e)
    {
        AddIncoming(new ChannelMessageViewModel(DateTime.Now, e.Data, _session.User));
    }

    private void Session_MessageSent(object? sender, Common.DataEventArgs<string> e)
    {
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Data, _session.Server.UserNick));
    }

    private void Session_ActionReceived(object? sender, Common.DataEventArgs<string> e)
    {
        AddIncoming(new ChannelActionViewModel(DateTime.Now, e.Data, _session.User));
    }

    private void Session_ActionSent(object? sender, Common.DataEventArgs<string> e)
    {
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, e.Data, _session.Server.UserNick));
    }

    private void Session_NoticeSent(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new OutgoingNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Server_OnNick(object? sender, NickChangeEventArgs e)
    {
        if (string.Equals(e.User.Nick, OtherNick, StringComparison.OrdinalIgnoreCase))
        {
            _session.User.Nick = e.NewNick;
            OtherNick = e.NewNick;
            Name = e.NewNick;
            ChatLines.Add(new ChatItemViewModel(DateTime.Now,
                ChannelStrings.NickChange.With(e.User.Nick, e.NewNick)));
        }
    }

    private void Server_Disconnected(object? sender, EventArgs e)
    {
        IsServerConnected = false;
        ChatLines.Add(new ChatItemViewModel(DateTime.Now, ServerStrings.Disconnected));
    }

    private void Server_ConnectionLost(object? sender, DisconnectEventArgs e)
    {
        IsServerConnected = false;
    }

    private void Server_Connected(object? sender, EventArgs e)
    {
        IsServerConnected = true;
    }

    protected override void OnExecute(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return;

        CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(_session, commandLine));
        if (result != null && result.Result == Result.Fail)
        {
            ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
        }
    }

    public override void Dispose()
    {
        if (_session == null) return;

        _session.MessageReceived -= Session_MessageReceived;
        _session.MessageSent -= Session_MessageSent;
        _session.ActionReceived -= Session_ActionReceived;
        _session.ActionSent -= Session_ActionSent;
        _session.NoticeSent -= Session_NoticeSent;

        _session.Server.OnNick -= Server_OnNick;
        _session.Server.Disconnected -= Server_Disconnected;
        _session.Server.Connected -= Server_Connected;
        _session.Server.ConnectionLost -= Server_ConnectionLost;
    }
}
