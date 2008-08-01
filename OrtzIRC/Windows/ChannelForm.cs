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
        private string ChannelName;
        private Channel Channel;

        public ChannelForm(Server parent, string channelName)
        {
            InitializeComponent();

            Server = parent;
            ChannelName = channelName;

            this.Text = channelName;
            this.MdiParent = parent.ServerView.MdiParent;
            this.commandTextBox.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Server.Connection.Sender.Part(ChannelName);
        }

        public void AppendLine(string line)
        {
            channelOutputBox.AppendLine(line);
        }

        public void AddNick(string nick)
        {
            nickListBox.AddNick(nick);
        }

        public void ResetNicks()
        {
            nickListBox.ResetNicks();
        }
    }
}
