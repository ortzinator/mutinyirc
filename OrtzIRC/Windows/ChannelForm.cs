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

        public ChannelForm(Channel channel, Server server, string channelName)
        {
            InitializeComponent();

            Server = server;
            ChannelName = channelName;
            Channel = channel;

            this.Text = channelName;
            this.nickListBox.DataSource = channel.NickList;

            this.commandTextBox.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Server.Connection.Sender.Part(ChannelName);
        }

        public void AddLine(string line)
        {
            channelOutputBox.AddLine(line);
            //channelOutputBox.AddLine(this.nickListBox.BindingContext.ToString());
        }
    }
}
