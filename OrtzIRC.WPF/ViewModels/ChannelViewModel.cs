using OrtzIRC.PluginFramework;

namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.WPF.Resources;

    public class ChannelViewModel : IrcViewModel
    {
        private Channel channel;
        private List<UserViewModel> userList;

        public new string Name
        {
            get
            {
                if (UserList == null || UserList.Count == 0)
                    return base.Name;
                return string.Format("{0} ({1})", base.Name, UserList.Count);
            }
        }

        public List<UserViewModel> UserList
        {
            get
            {
                return userList;
            }
        }

        public ChannelViewModel(Channel chan)
        {
            channel = chan;
            channel.NickList.Updated += NickList_Updated;
            base.Name = chan.Name;

            channel.OnMessage += Channel_OnMessage;
            channel.OnAction += Channel_OnAction;
            channel.TopicReceived += Channel_TopicReceived;
            channel.OnJoin += Channel_OnJoin;
            channel.UserParted += Channel_UserParted;
            channel.OtherUserParted += Channel_OtherUserParted;
            channel.UserQuitted += Channel_OnUserQuitted;
            channel.NickChanged += Channel_OnNick;
            channel.OnKick += Channel_OnKick;
            channel.MessagedChannel += Channel_MessagedChannel;

            channel.Server.Disconnected += Server_Disconnected;
        }

        private void Server_Disconnected(object sender, EventArgs e)
        {
            AddMessage(ServerStrings.Disconnected);
            //TODO: Close
        }

        private void Channel_MessagedChannel(object sender, UserMessageEventArgs e)
        {
            ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Message, e.User));
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            AddMessage(ChannelStrings.Kick.With(kickee, nick.Nick, reason));
        }

        private void Channel_OnNick(object sender, NickChangeEventArgs e)
        {
            AddMessage(ChannelStrings.NickChange.With(e.User.Nick, e.NewNick));
        }

        private void Channel_OnUserQuitted(object sender, UserMessageEventArgs e)
        {
            AddMessage(ChannelStrings.Quit.With(e.User.Nick, e.User.HostMask, e.Message));
        }

        private void Channel_OtherUserParted(object sender, UserMessageEventArgs e)
        {
            if (e.Message == String.Empty)
                AddMessage(ChannelStrings.Part.With(e.User.Nick, e.User.HostMask));
            else
                AddMessage(ChannelStrings.PartWithReason.With(e.User.Nick, e.User.HostMask, e.Message));
        }

        private void Channel_UserParted(object sender, EventArgs e)
        {
            Close();
        }

        private void Channel_OnJoin(object sender, UserEventArgs e)
        {
            AddMessage(ChannelStrings.Joined.With(e.User.Nick, e.User.HostMask));
        }

        private void Channel_TopicReceived(object sender, DataEventArgs<string> e)
        {
            AddMessage(ChannelStrings.TopicRecieved.With(e.Data));
        }

        private void Channel_OnAction(object sender, UserMessageEventArgs e)
        {
            ChatLines.Add(new ChannelActionViewModel(DateTime.Now, e.Message, e.User));
        }

        private void Channel_OnMessage(object sender, UserMessageEventArgs e)
        {
            ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, e.Message, e.User));
        }

        private void NickList_Updated(object sender, EventArgs e)
        {
            userList = new List<UserViewModel>();
            foreach (User user in channel.NickList)
            {
                userList.Add(new UserViewModel(user));
            }
            userList.Sort((user1, user2) => user1.CompareTo(user2));
            RaisePropertyChanged("UserList");
            RaisePropertyChanged("Name");
        }

        protected override void OnExecute(string commandLine)
        {
            //ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, commandLine, new User { Nick = "Ortzinator" }));
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(channel, commandLine));
            if (result != null && result.Result == CommandResult.Fail)
            {
                ChatLines.Add(new ErrorMessageViewModel(DateTime.Now, result.Message));
                //TODO: Log
            }
        }

        private void AddMessage(string msg)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, msg));
        }

        public override void Dispose()
        {
            channel.NickList.Updated -= NickList_Updated;

            channel.OnMessage -= Channel_OnMessage;
            channel.OnAction -= Channel_OnAction;
            channel.TopicReceived -= Channel_TopicReceived;
            channel.OnJoin -= Channel_OnJoin;
            channel.UserParted -= Channel_UserParted;
            channel.OtherUserParted -= Channel_OtherUserParted;
            channel.UserQuitted -= Channel_OnUserQuitted;
            channel.NickChanged -= Channel_OnNick;
            channel.OnKick -= Channel_OnKick;
            channel.MessagedChannel -= Channel_MessagedChannel;

            channel.Server.Disconnected -= Server_Disconnected;
        }
    }
}
