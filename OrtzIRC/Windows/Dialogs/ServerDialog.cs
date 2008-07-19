using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OrtzIRC.Properties;
using System.Collections;

namespace OrtzIRC.Dialogs
{
    public partial class ServerDialog : Form
    {
        ServerManager _serverManager;
        public ServerDialog()
        {
            _serverManager = ServerManager.Instance();
            this.FormClosing += new FormClosingEventHandler(ServerDialog_FormClosing);
            InitializeComponent();
        }

        void ServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Settings.Default.Save();
        }

        private void ServerDialog_Load(object sender, EventArgs e)
        {
            //foreach (Server server in Settings.Default.ServerList)
            {
                //serverListBox.Items.Add(server.Description);
            }
            //serverListBox.Items.Add("goo");
            //serverListBox.Items.Add("goo");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Server temp = new Server("irc.gamesurge.net", "Hello new server", 6667);

            //Settings.Default.ServerList.Add(temp);
        }
    }
}