using System.Collections.Generic;

namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    public partial class ChannelForm : Form
    {
        private Server Server;

        public ChannelForm(Channel channel)
        {
            InitializeComponent();

            Server = channel.Server;
            Channel = channel;

            Text = channel.Name;

            Channel.OnMessage += Channel_OnMessage;
            Channel.OnAction += Channel_OnAction;
            Channel.OnShowTopic += Channel_OnShowTopic;
            Channel.OnJoin += Channel_OnJoin;
            Channel.OnUserPart += Channel_OnPartOther;
            Channel.OnUserQuit += Channel_OnUserQuit;
            Channel.OnNick += Channel_OnNick;
            Channel.OnKick += Channel_OnKick;
            Channel.MessagedChannel += Channel_MessagedChannel;
            Server.Disconnected += Server_Disconnected;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;

            nickListBox.UserList = Channel.NickList;

            commandTextBox.Focus();
        }

        public Channel Channel { get; private set; }

        private void Channel_MessagedChannel(object sender, DataEventArgs<string> e)
        {
            AddLine(string.Format("{0}: {1}", Server.UserNick, e.Data));
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(Channel, e.Data));
        }

        private void Server_Disconnected()
        {
            Close();
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            AddLine(string.Format("-- Kick: ({0}) was kicked by ({1}) {2}", nick.Nick, kickee, reason));
        }

        private void Channel_OnNick(User nick, string newNick)
        {
            AddLine(string.Format("-- Nick: ({0}) is now known as ({1})", nick.Nick, newNick));
        }

        private void Channel_OnUserQuit(User nick, string message)
        {
            AddLine(string.Format("-- Quit: ({0}) ({1}) {2}", nick.Nick, nick.HostMask, message));
        }

        private void Channel_OnPartOther(User nick, string message)
        {
            if (message != String.Empty)
                AddLine(string.Format("-- Parted: ({0}) ({1}) {2}", nick.Nick, nick.HostMask, message));
            else
                AddLine(string.Format("-- Parted: ({0}) ({1})", nick.Nick, nick.HostMask));
        }

        private void Channel_OnJoin(User nick)
        {
            AddLine(string.Format("-- Joined: ({0}) ({1})", nick.Nick, nick.HostMask));
        }

        private void Channel_OnShowTopic(string topic)
        {
            AddLine(string.Format("topic: ({0})", topic));
        }

        private void Channel_OnAction(User nick, string message)
        {
            AddLine(string.Format("-- {0} {1}", nick.Nick, message));
        }

        private void Channel_OnMessage(User nick, string message)
        {
            AddLine(string.Format("{0}: {1}", nick.NamesLiteral, message));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Channel.Part();
        }

        public void AddLine(string line)
        {
            channelOutputBox.AppendLine(line);

            TextLoggerManager.TextEntry(Channel, line + '\n');
        }
    }
}