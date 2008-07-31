namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using OrtzIRC.Properties;
    using System.Collections;

    public partial class MainForm : Form
    {
        public static List<ServerForm> ServerList { get; private set; }

        public MainForm()
        {
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            this.Load += new EventHandler(MainForm_Load);
            MainForm.ServerList = new List<ServerForm>();
            LoadServerList();

            InitializeComponent();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ServerSettings settings = new ServerSettings("openircnet.ath.cx", "what", 6667, false);
                ServerForm server = new ServerForm(settings);
                server.MdiParent = this;
                server.Show();
            }
        }

        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Settings.Default.Save();
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