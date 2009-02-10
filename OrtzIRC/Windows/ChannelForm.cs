namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    public partial class ChannelForm : Form
    {
        private Server Server;
        public Channel Channel { get; private set; }
        private string ChannelName;

        public ChannelForm(Channel channel)
        {
            InitializeComponent();

            Server = channel.Server;
            Channel = channel;
            ChannelName = channel.Name;

            this.Text = channel.Name;

            Channel.OnMessage += new ChannelMessageEventHandler(Channel_OnMessage);
            Channel.OnAction += new ChannelMessageEventHandler(Channel_OnAction);
            Channel.OnShowTopic += new TopicShowEventHandler(Channel_OnShowTopic);
            Channel.OnJoin += new ChannelJoinEventHandler(Channel_OnJoin);
            Channel.OnUserPart += new ChannelPartOtherEventHandler(Channel_OnPartOther);
            Channel.OnUserQuit += new ChannelQuitEventHandler(Channel_OnUserQuit);
            Channel.OnNick += new Server_NickEventHandler(Channel_OnNick);
            Channel.OnKick += new ChannelKickEventHandler(Channel_OnKick);
            Channel.MessagedChannel += new EventHandler<DataEventArgs<string>>(Channel_MessagedChannel);
            Server.Disconnected += new DisconnectedEventHandler(Server_Disconnected);

            commandTextBox.CommandEntered += new EventHandler<DataEventArgs<string>>(commandTextBox_CommandEntered);

            nickListBox.UserList = Channel.NickList;

            this.commandTextBox.Focus();
        }

        private void Channel_MessagedChannel(object sender, DataEventArgs<string> e)
        {
            this.AddLine(this.Server.UserNick + ": " + e.Data);
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            if (e.Data.StartsWith("/"))
            {
                string[] exploded = e.Data.Split(new Char[] { ' ' });
                string name = exploded[0].TrimStart('/');
                string[] parameters = new string[exploded.Length - 1];
                Array.Copy(exploded, 1, parameters, 0, exploded.Length - 1); //Removing the first element
                PluginManager.GetCommandInstance(this.Channel, name, parameters);
            }
            else
            {
                string[] parameters = e.Data.Split(new Char[] { ' ' });
                PluginManager.GetCommandInstance(this.Channel, "say", parameters);
            }
        }

        private void Server_Disconnected()
        {
            this.Close();
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            this.AddLine("-- Kick: (" + nick.Nick + ") was kicked by (" + kickee + ") " + reason);
        }

        private void Channel_OnNick(User nick, string newNick)
        {
            this.AddLine("-- Nick: (" + nick.Nick + ") is now known as (" + newNick + ")");
        }

        private void Channel_OnUserQuit(User nick, string message)
        {
            this.AddLine("-- Quit: (" + nick.Nick + ") (" + nick.HostMask + ") " + message);
        }

        private void Channel_OnPartOther(User nick, string message)
        {
            if (message != String.Empty)
                this.AddLine("-- Parted: (" + nick.Nick + ") (" + nick.HostMask + ") " + message);
            else
                this.AddLine("-- Parted: (" + nick.Nick + ") (" + nick.HostMask + ")");
        }

        private void Channel_OnJoin(User nick)
        {
            this.AddLine("-- Joined: (" + nick.Nick + ") (" + nick.HostMask + ")");
        }

        private void Channel_OnShowTopic(string topic)
        {
            this.AddLine("topic: (" + topic + ")");
        }

        private void Channel_OnAction(User nick, string message)
        {
            this.AddLine("-- " + nick.Nick + " " + message);
        }

        private void Channel_OnMessage(User nick, string message)
        {
            this.AddLine(nick.NamesLiteral + ": " + message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Channel.Part("Leaving"); //TODO: Get a default part message
        }

        public void AddLine(string line)
        {
            channelOutputBox.AppendLine(line);
        }
    }
}
