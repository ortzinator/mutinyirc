using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC
{
    public partial class ChannelForm : Form
    {
        private Server Server;
        private Channel Channel;
        private string ChannelName;

        public ChannelForm(Channel channel, Server server)
        {
            InitializeComponent();

            Server = server;
            Channel = channel;
            ChannelName = channel.Name;

            this.Text = channel.Name;
            this.nickListBox.DataSource = channel.NickList;

            Channel.OnMessage += new ChannelMessageEventHandler(Channel_OnMessage);
            Channel.OnAction += new ChannelMessageEventHandler(Channel_OnAction);
            Channel.OnShowTopic += new TopicShowEventHandler(Channel_OnShowTopic);

            this.commandTextBox.Focus();
        }

        void Channel_OnShowTopic(string topic)
        {
            this.AddLine("topic: (" + topic + ")");
        }

        void Channel_OnAction(Nick nick, string message)
        {
            this.AddLine("--" + nick.Name + " " + message);
        }

        void Channel_OnMessage(Nick nick, string message)
        {
            this.AddLine(nick.Name + ": " + message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Server.Connection.Sender.Part(ChannelName);
        }

        public void AddLine(string line)
        {
            channelOutputBox.AddLine(line);
        }
    }
}
