namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;
    using System.IO;

    public partial class MainForm : Form
    {
        public static List<Server> ServerList { get; private set; }
        //TODO: ServerManager

        public MainForm()
        {
            InitializeComponent();

            MainForm.ServerList = new List<Server>();

            LoadServerList();
        }

        protected override void OnLoad(EventArgs e)
        {
            //hack
            Settings.Default.UserPluginDirectory = Path.Combine(Environment.CurrentDirectory, @"Plugins");
            Settings.Default.Save();

            if (!Directory.Exists(Settings.Default.UserPluginDirectory))
                Directory.CreateDirectory(Settings.Default.UserPluginDirectory);

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Server newServer = ServerManager.Instance.Create("irc.randomirc.com", "RandomIRC", 6667, false);
                ServerForm newServerForm = new ServerForm() { Server = newServer };
                newServerForm.MdiParent = this;
                newServerForm.Text = "RandomIRC";
                newServerForm.Show();
            }

            PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);

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
            Dialogs.ServerDialog servers = new Dialogs.ServerDialog();
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