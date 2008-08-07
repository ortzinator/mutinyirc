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
        public Server ParentServer { get; private set; }

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm(Server parent)
        {
            InitializeComponent();

            ParentServer = parent;

            ParentServer.OnDisconnected += new DisconnectedEventHandler(ParentServer_OnDisconnected);
            ParentServer.OnPublicMessage += new Server_PublicMessageEventHandler(ParentServer_OnPublicMessage);

            this.commandTextBox.Focus();
        }

        void ParentServer_OnPublicMessage(Nick nick, Channel chan, string message)
        {
            throw new NotImplementedException();
        }

        void ParentServer_OnDisconnected()
        {
            //TODO: Close all channel windows
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (ParentServer.Connection.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ParentServer.Connection.Disconnect("Quitan");
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
            serverOutputBox.AddLine(line);
        }
    }
}