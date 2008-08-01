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
        private Server _parent;
        private string _channel;

        public ChannelForm(Server parent, string channelName)
        {
            InitializeComponent();

            _parent = parent;
            _channel = channelName;

            this.FormClosing += new FormClosingEventHandler(ChannelForm_FormClosing);
        }

        void ChannelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Connection.Sender.Part(_channel);
        }

        public void AppendLine(string line)
        {
            channelOutputBox.AppendLine(line);
        }

        internal void AddNick(string nick)
        {
            nickListBox.AddNick(nick);
        }

        internal void ResetNicks()
        {
            nickListBox.ResetNicks();
        }
    }
}
