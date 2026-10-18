
using System;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI.ViewModels;

public class PrivateMessageViewModel : IrcViewModel
{
    // IDE0370 is a false positive here: without the '!' the build gives CS8625.
#pragma warning disable IDE0370
    public PrivateMessageSession Session { get; } = null!;
    private readonly PluginManager _pluginManager = null!;
#pragma warning restore IDE0370

    public override Server? OwningServer => Session.Server;

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
        if (Avalonia.Controls.Design.IsDesignMode)
            return;

        Session = session;
        _pluginManager = pluginManager;
        OtherNick = session.User.Nick;
        Name = session.User.Nick;
        _isServerConnected = session.Server.IsConnected;

        Session.MessageReceived += Session_MessageReceived;
        Session.MessageSent += Session_MessageSent;
        Session.ActionReceived += Session_ActionReceived;
        Session.ActionSent += Session_ActionSent;
        Session.NoticeSent += Session_NoticeSent;

        Session.Server.OnNick += Server_OnNick;
        Session.Server.Disconnected += Server_Disconnected;
        Session.Server.Connected += Server_Connected;
        Session.Server.ConnectionLost += Server_ConnectionLost;
    }

    private void Session_MessageReceived(object? sender, Common.DataEventArgs<string> e)
    {
        AddIncoming(new ChannelMessageViewModel(DateTime.Now, e.Data, Session.User));
    }

    private void Session_MessageSent(object? sender, Common.DataEventArgs<string> e)
    {
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Data, Session.Server.UserNick));
    }

    private void Session_ActionReceived(object? sender, Common.DataEventArgs<string> e)
    {
        AddIncoming(new ChannelActionViewModel(DateTime.Now, e.Data, Session.User));
    }

    private void Session_ActionSent(object? sender, Common.DataEventArgs<string> e)
    {
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, e.Data, Session.Server.UserNick));
    }

    private void Session_NoticeSent(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new OutgoingNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Server_OnNick(object? sender, NickChangeEventArgs e)
    {
        if (string.Equals(e.User.Nick, OtherNick, StringComparison.OrdinalIgnoreCase))
        {
            Session.User.Nick = e.NewNick;
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

        CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(Session, commandLine));
        if (result != null && result.Result == Result.Fail)
        {
            ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
        }
    }

    public override void Dispose()
    {
        if (Session == null) return;

        Session.MessageReceived -= Session_MessageReceived;
        Session.MessageSent -= Session_MessageSent;
        Session.ActionReceived -= Session_ActionReceived;
        Session.ActionSent -= Session_ActionSent;
        Session.NoticeSent -= Session_NoticeSent;

        Session.Server.OnNick -= Server_OnNick;
        Session.Server.Disconnected -= Server_Disconnected;
        Session.Server.Connected -= Server_Connected;
        Session.Server.ConnectionLost -= Server_ConnectionLost;
    }
}
