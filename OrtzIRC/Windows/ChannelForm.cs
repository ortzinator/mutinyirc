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

            HookEvents();

            nickListBox.UserList = Channel.NickList;

            commandTextBox.Focus();
        }

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

            Server.Disconnected += Server_Disconnected;

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

            Server.Disconnected -= Server_Disconnected;

            commandTextBox.CommandEntered -= commandTextBox_CommandEntered;
        }

        public Channel Channel { get; private set; }

        private void Channel_MessagedChannel(object sender, UserMessageEventArgs e)
        {
            AddLine(string.Format("{0}: {1}", e.User.NamesLiteral, e.Message));
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(Channel, e.Data)); //TODO: Do something with the result
            if (result != null && result.Result == CommandResult.Fail)
            {
                channelOutputBox.AppendError("\n" + result.Message);
            }
        }

        private void Server_Disconnected()
        {
            AddLine("--Disconnected--");
            Close();
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            AddLine(string.Format("-- Kick: ({0}) was kicked by ({1}) {2}", kickee, nick.Nick, reason));
        }

        private void Channel_OnNick(User nick, string newNick)
        {
            AddLine(string.Format("-- Nick: ({0}) is now known as ({1})", nick.Nick, newNick));
        }

        private void Channel_OnUserQuitted(object sender, UserMessageEventArgs e)
        {
            AddLine(string.Format("-- Quit: ({0}) ({1}) {2}", e.User.Nick, e.User.HostMask, e.Message));
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
                AddLine(string.Format("-- Parted: ({0}) ({1})", e.User.Nick, e.User.HostMask));
            else
                AddLine(string.Format("-- Parted: ({0}) ({1}) {2}", e.User.Nick, e.User.HostMask, e.Message));
        }

        private void Channel_OnJoin(object sender, UserEventArgs e)
        {
            AddLine(string.Format("-- Joined: ({0}) ({1})", e.User.Nick, e.User.HostMask));
        }

        private void Channel_TopicReceived(object sender, DataEventArgs<string> e)
        {
            AddLine(string.Format("topic: ({0})", e.Data));
        }

        private void Channel_OnAction(object sender, UserMessageEventArgs e)
        {
            AddLine(string.Format("-- {0} {1}", e.User.Nick, e.Message));
        }

        private void Channel_OnMessage(object sender, UserMessageEventArgs e)
        {
            AddLine(string.Format("{0}: {1}", e.User.NamesLiteral, e.Message));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ChannelForm Closing: " + e.CloseReason);

            if (e.CloseReason == CloseReason.MdiFormClosing) return;

            UnhookEvents();
            Channel.Part();
        }

        public void AddLine(string line)
        {
            channelOutputBox.AppendLine(line);

            TextLoggerManager.TextEntry(Channel, line + '\n');
        }
    }
}