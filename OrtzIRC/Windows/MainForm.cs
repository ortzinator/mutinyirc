using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OrtzIRC.Properties;
using System.Collections;

namespace OrtzIRC
{
    public partial class MainForm : Form
    {
        public static List<Server> ServerList { get; private set; }
        //TODO: ServerManager

        public MainForm()
        {
            MainForm.ServerList = new List<Server>();

            LoadServerList();

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

                ServerSettings settings = new ServerSettings("chat.freenode.net", "Freenode", 6667, false);
                Server newServer = ServerManager.Instance.Create(settings);
                ServerForm newServerForm = new ServerForm(newServer);
                newServerForm.MdiParent = this;
                newServerForm.Text = settings.Uri;
                newServerForm.Show();

                //Server server = new Server(settings, this);
            }

            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //Settings.Default.Save();
            base.OnClosing(e);
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newServerMenuItem_Click(object sender, EventArgs e)
        {
            OrtzIRC.Dialogs.ServerDialog servers = new OrtzIRC.Dialogs.ServerDialog();
            servers.Show();
        }

        private void LoadServerList()
        {
            //if (Settings.Default.ServerList == null)
            {
                // Settings.Default.ServerList = new System.Collections.ArrayList();
            }
        }
    }
}