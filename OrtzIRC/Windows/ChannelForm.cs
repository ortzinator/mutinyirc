﻿using System;
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

        private delegate void SyncDelegate();

        public ChannelForm(Channel channel, Server server, string channelName)
        {
            InitializeComponent();

            Server = server;
            ChannelName = channelName;
            Channel = channel;

            this.Text = channelName;
            this.nickListBox.DataSource = channel.NickList;

            this.Server.ServerView.Invoke(new SyncDelegate(SetMdi));

            this.commandTextBox.Focus();
        }

        private void SetMdi()
        {
            this.MdiParent = this.Server.ServerView.MdiParent;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Server.Connection.Sender.Part(ChannelName);
        }

        public void AppendLine(string line)
        {
            channelOutputBox.AppendLine(line);
            //channelOutputBox.AppendLine(this.nickListBox.BindingContext.ToString());
        }
    }
}
