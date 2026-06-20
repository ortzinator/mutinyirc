using Ninject;

namespace MutinyIRC.UI.ViewModels;

using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using FlamingIRC;
using Common;
using PluginFramework;
using Resources;

public class ChannelViewModel : IrcViewModel
{
    private Channel _channel;
    public Channel Channel => _channel;
    public override Server? OwningServer => _channel.Server;
    private List<UserViewModel> userList = new();
    private PluginManager _pluginManager;

    public PluginManager PluginManager => _pluginManager;

    public string ChannelName => base.Name.TrimStart('#', '&', '+', '!');

    private string _topic = string.Empty;
    public string Topic
    {
        get => _topic;
        private set => SetProperty(ref _topic, value);
    }

    public new string Name
    {
        get
        {
            if (UserList == null || UserList.Count == 0)
                return base.Name;
            return string.Format("{0} ({1})", base.Name, UserList.Count);
        }
    }

    public List<UserViewModel> UserList => userList;

    public ChannelViewModel(Channel channel, PluginManager pluginManager)
    {
        _pluginManager = pluginManager;

        _channel = channel;
        _channel.Users.Updated += NickList_Updated;
        base.Name = channel.Name;

        _channel.OnMessage += Channel_OnMessage;
        _channel.OnAction += Channel_OnAction;
        _channel.TopicReceived += Channel_TopicReceived;
        _channel.OnJoin += Channel_OnJoin;
        _channel.UserParted += Channel_UserParted;
        _channel.OtherUserParted += Channel_OtherUserParted;
        _channel.UserQuitted += Channel_OnUserQuitted;
        _channel.NickChanged += Channel_OnNick;
        _channel.OnKick += Channel_OnKick;
        _channel.MessagedChannel += Channel_MessagedChannel;
        _channel.NoticeSent += Channel_NoticeSent;
        _channel.OnNotice += Channel_OnNotice;

        _channel.Server.Disconnected += Server_Disconnected;
    }

    private void Channel_NoticeSent(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new OutgoingNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Channel_OnNotice(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
    }

    private void Server_Disconnected(object? sender, EventArgs e)
    {
        AddMessage(ServerStrings.Disconnected);
        Close();
    }

    private void Channel_MessagedChannel(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Message, e.User));
    }

    private void Channel_OnKick(object? sender, KickEventArgs e)
    {
        AddMessage(ChannelStrings.Kick.With(e.Kickee, e.User.Nick, e.Reason));
    }

    private void Channel_OnNick(object? sender, NickChangeEventArgs e)
    {
        AddMessage(ChannelStrings.NickChange.With(e.User.Nick, e.NewNick));
    }

    private void Channel_OnUserQuitted(object? sender, UserMessageEventArgs e)
    {
        AddMessage(ChannelStrings.Quit.With(e.User.Nick, e.User.HostMask, e.Message));
    }

    private void Channel_OtherUserParted(object? sender, UserMessageEventArgs e)
    {
        if (e.Message == string.Empty)
            AddMessage(ChannelStrings.Part.With(e.User.Nick, e.User.HostMask));
        else
            AddMessage(ChannelStrings.PartWithReason.With(e.User.Nick, e.User.HostMask, e.Message));
    }

    private void Channel_UserParted(object? sender, EventArgs e)
    {
        Close();
    }

    private void Channel_OnJoin(object? sender, UserEventArgs e)
    {
        AddMessage(ChannelStrings.Joined.With(e.User.Nick, e.User.HostMask));
    }

    private void Channel_TopicReceived(object? sender, Common.DataEventArgs<string> e)
    {
        Topic = e.Data;
        AddMessage(ChannelStrings.TopicRecieved.With(e.Data));
    }

    private void Channel_OnAction(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new ChannelActionViewModel(DateTime.Now, e.Message, e.User));
    }

    private void Channel_OnMessage(object? sender, UserMessageEventArgs e)
    {
        ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Message, e.User));
    }

    private void NickList_Updated(object? sender, EventArgs e)
    {
        userList = new List<UserViewModel>();
        foreach (User user in _channel.Users)
            userList.Add(new UserViewModel(user));
        userList.Sort((user1, user2) => user1.CompareTo(user2));
        OnPropertyChanged("UserList");
        OnPropertyChanged("Name");
    }

    protected override void OnExecute(string? commandLine)
    {
        if (string.IsNullOrEmpty(commandLine))
            return;

        CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(_channel, commandLine));
        if (result != null && result.Result == Result.Fail)
        {
            ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
        }
    }

    // ── User-list context-menu actions ──
    // Each routes through the same dispatcher as typed commands so behaviour stays identical.

    private RelayCommand<UserViewModel>? _whoisUserCommand;
    public System.Windows.Input.ICommand WhoisUserCommand => _whoisUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/whois", u));

    private RelayCommand<UserViewModel>? _queryUserCommand;
    public System.Windows.Input.ICommand QueryUserCommand => _queryUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/query", u));

    private RelayCommand<UserViewModel>? _opUserCommand;
    public System.Windows.Input.ICommand OpUserCommand => _opUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/mode +o", u));

    private RelayCommand<UserViewModel>? _deopUserCommand;
    public System.Windows.Input.ICommand DeopUserCommand => _deopUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/mode -o", u));

    private RelayCommand<UserViewModel>? _voiceUserCommand;
    public System.Windows.Input.ICommand VoiceUserCommand => _voiceUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/mode +v", u));

    private RelayCommand<UserViewModel>? _devoiceUserCommand;
    public System.Windows.Input.ICommand DevoiceUserCommand => _devoiceUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/mode -v", u));

    private RelayCommand<UserViewModel>? _kickUserCommand;
    public System.Windows.Input.ICommand KickUserCommand => _kickUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/kick", u));

    private RelayCommand<UserViewModel>? _banUserCommand;
    public System.Windows.Input.ICommand BanUserCommand => _banUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/ban", u));

    private RelayCommand<UserViewModel>? _kickBanUserCommand;
    public System.Windows.Input.ICommand KickBanUserCommand => _kickBanUserCommand ??= new RelayCommand<UserViewModel>(u => RunUserCommand("/ban -k", u));

    private void RunUserCommand(string commandPrefix, UserViewModel? user)
    {
        if (user == null)
            return;

        OnExecute($"{commandPrefix} {user.Nick}");
    }

    private void AddMessage(string msg)
    {
        ChatLines.Add(new ChatItemViewModel(DateTime.Now, msg));
    }

    public override void Dispose()
    {
        _channel.Users.Updated -= NickList_Updated;

        _channel.OnMessage -= Channel_OnMessage;
        _channel.OnAction -= Channel_OnAction;
        _channel.TopicReceived -= Channel_TopicReceived;
        _channel.OnJoin -= Channel_OnJoin;
        _channel.UserParted -= Channel_UserParted;
        _channel.OtherUserParted -= Channel_OtherUserParted;
        _channel.UserQuitted -= Channel_OnUserQuitted;
        _channel.NickChanged -= Channel_OnNick;
        _channel.OnKick -= Channel_OnKick;
        _channel.MessagedChannel -= Channel_MessagedChannel;
        _channel.NoticeSent -= Channel_NoticeSent;
        _channel.OnNotice -= Channel_OnNotice;

        _channel.Server.Disconnected -= Server_Disconnected;
    }
}
