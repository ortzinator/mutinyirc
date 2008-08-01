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

            this.commandTextBox.Focus();
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
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
        }

        public void AppendLine(string line)
        {

            if (this.serverOutputBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AppendLine);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                serverOutputBox.Text += "\n" + line.Trim();
            }
        }

        

    }
}