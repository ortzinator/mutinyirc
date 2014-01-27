﻿using Ninject;

namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Resources;
    using System.Drawing;

    public partial class ChannelForm : Form
    {
        private PluginManager _pluginManager;

        [Inject]
        public ChannelForm(Channel channel, PluginManager pluginManager)
        {
            InitializeComponent();

            Channel = channel;

            HookEvents();

            nickListBox.UserList = Channel.Users;

            commandTextBox.Focus();
            _pluginManager = pluginManager;
        }

        public Channel Channel { get; private set; }

        private void HookEvents()
        {
            Load += delegate
            {
                Text = Channel.Name;
            };

            Channel.OnMessage += Channel_OnMessage;
            Channel.OnAction += Channel_OnAction;
            Channel.TopicReceived += Channel_TopicReceived;
            Channel.OnJoin += Channel_OnJoin;
            Channel.UserParted += Channel_UserParted;
            Channel.OtherUserParted += Channel_OtherUserParted;
            Channel.UserQuitted += Channel_OnUserQuitted;
            Channel.NickChanged += Channel_OnNick;
            Channel.OnKick += Channel_OnKick;
            Channel.MessagedChannel += Channel_MessagedChannel;

            Channel.Server.Disconnected += Server_Disconnected;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;
            channelOutputBox.MouseUp += channelOutputBox_MouseUp;
        }

        private void channelOutputBox_MouseUp(object sender, MouseEventArgs e)
        {
            commandTextBox.Focus();
        }

        private void UnhookEvents()
        {
            Load -= delegate
            {
                Text = Channel.Name;
            };

            Channel.OnMessage -= Channel_OnMessage;
            Channel.OnAction -= Channel_OnAction;
            Channel.TopicReceived -= Channel_TopicReceived;
            Channel.OnJoin -= Channel_OnJoin;
            Channel.UserParted -= Channel_UserParted;
            Channel.OtherUserParted -= Channel_OtherUserParted;
            Channel.UserQuitted -= Channel_OnUserQuitted;
            Channel.NickChanged -= Channel_OnNick;
            Channel.OnKick -= Channel_OnKick;
            Channel.MessagedChannel -= Channel_MessagedChannel;

            Channel.Server.Disconnected -= Server_Disconnected;

            commandTextBox.CommandEntered -= commandTextBox_CommandEntered;
        }

        private void Channel_MessagedChannel(object sender, UserMessageEventArgs e)
        {
            AddLine(ChannelStrings.PublicMessage.With(e.User.NamesLiteral, e.Message));
        }

        private void commandTextBox_CommandEntered(object sender, Common.DataEventArgs<string> e)
        {
            CommandResultInfo result = _pluginManager.ExecuteCommand(_pluginManager.ParseCommand(Channel, e.Data));
            if (result != null && result.Result == Result.Fail)
            {
                channelOutputBox.AppendLine(result.Message, Color.Red);
                //TODO: Log
            }
        }

        private void Server_Disconnected(object sender, EventArgs e)
        {
            AddLine(ServerStrings.Disconnected);
            Close();
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            AddLine(ChannelStrings.Kick.With(kickee, nick.Nick, reason));
        }

        private void Channel_OnNick(object sender, NickChangeEventArgs ea)
        {
            AddLine(ChannelStrings.NickChange.With(ea.User.Nick, ea.NewNick));
        }

        private void Channel_OnUserQuitted(object sender, UserMessageEventArgs e)
        {
            AddLine(ChannelStrings.Quit.With(e.User.Nick, e.User.HostMask, e.Message));
        }

        private void Channel_UserParted(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action(Close));
            else
                Close();
        }

        private void Channel_OtherUserParted(object sender, UserMessageEventArgs e)
        {
            if (e.Message == String.Empty)
                AddLine(ChannelStrings.Part.With(e.User.Nick, e.User.HostMask));
            else
                AddLine(ChannelStrings.PartWithReason.With(e.User.Nick, e.User.HostMask, e.Message));
        }

        private void Channel_OnJoin(object sender, UserEventArgs e)
        {
            AddLine(ChannelStrings.Joined.With(e.User.Nick, e.User.HostMask));
        }

        private void Channel_TopicReceived(object sender, Common.DataEventArgs<string> e)
        {
            AddLine(ChannelStrings.TopicRecieved.With(e.Data));
        }

        private void Channel_OnAction(object sender, UserMessageEventArgs e)
        {
            AddLine(ChannelStrings.Action.With(e.User.Nick, e.Message));
        }

        private void Channel_OnMessage(object sender, UserMessageEventArgs e)
        {
            AddLine(ChannelStrings.PublicMessage.With(e.User.NamesLiteral, e.Message));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ChannelForm Closing: " + e.CloseReason);

            if (e.CloseReason == CloseReason.MdiFormClosing) return;

            UnhookEvents();
            Channel.Part(); //TODO - Random part message
        }

        public void AddLine(string line)
        {
            channelOutputBox.AppendLine(line);

            TextLoggerManager.TextEntry(Channel, line + '\n');
        }

        protected override void OnEnter(EventArgs e)
        {
            commandTextBox.Focus();
            base.OnEnter(e);
        }
    }
}