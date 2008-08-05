using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;
using System.Collections;
using FlamingIRC;

namespace OrtzIRC
{
    public partial class ServerForm : Form
    {
        internal Server Server { get; private set; }

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm(Server parent)
        {
            InitializeComponent();

            Server = parent;

            this.commandTextBox.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (Server.Connection.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Server.Connection.Disconnect("Quitan");
                    Server.ChanManager.CloseAll();
                }
                else
                {
                    e.Cancel = true;
                }
            }

            base.OnFormClosing(e);
        }

        public void AppendLine(string line)
        {
            serverOutputBox.AppendLine(line);
        }
    }
}