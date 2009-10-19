namespace OrtzIRC
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //hack
            TextLoggerManager.LoggerActive = Settings.Default.LoggerActivated;
            TextLoggerManager.AddTimestamp = Settings.Default.LoggerTimestampsActivated;
            TextLoggerManager.TimeFormat = Settings.Default.LoggerTimestampFormat;

            windowManagerTreeView.AfterSelect += windowManagerTreeView_AfterSelect;

            Settings.Default.UserPluginDirectory = Path.Combine(Environment.CurrentDirectory, @"Plugins");
            Settings.Default.Save();

            IRCSettingsManager.Instance.GetAutoConnectServers();

            

            if (!Directory.Exists(Settings.Default.UserPluginDirectory))
                Directory.CreateDirectory(Settings.Default.UserPluginDirectory);

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var server in IRCSettingsManager.Instance.GetAutoConnectServers())
                {
                    Server newServer = ServerManager.Instance.Create(server);
                    CreateServerForm(newServer);
                    newServer.Connect();
                }
            }

            PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);

            base.OnLoad(e);
        }

        

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void serversMenuItem_Click(object sender, EventArgs e)
        {
            ServerSettingsDialog servers = new ServerSettingsDialog();
            servers.ShowDialog();
        }

        /// <summary>
        /// Creates and displays a ServerForm and adds a node to the WindowManagerTreeView
        /// </summary>
        /// <param name="server"></param>
        /// <returns>The ServerForm that was created</returns>
        public ServerForm CreateServerForm(Server server)
        {
            ServerForm newServerForm = new ServerForm { Server = server };

            Invoke((MethodInvoker)delegate
            {
                windowManagerTreeView.AddServerNode(new ServerTreeNode(newServerForm));
                newServerForm.MdiParent = this;
            });

            newServerForm.Text = server.Description;
            newServerForm.Show();

            return newServerForm;
        }

        private delegate void CreateChannelFormDlg(Channel channel);

        /// <summary>
        /// Creates and displays a ChannelForm and adds a node to the WindowManagerTreeView under the appropriate server node
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public void CreateChannelForm(Channel channel)
        {
            if (InvokeRequired)
            {
                Invoke((CreateChannelFormDlg)CreateChannelForm, new object[] { channel });
            }
            else
            {
                ChannelForm newChannelForm = new ChannelForm(channel);
                ServerTreeNode node = windowManagerTreeView.GetServerNode(channel.Server);

                if (node == null)
                    throw new Exception("ServerTreeNode doesn't exist!"); //hack

                node.AddChannelNode(new ChannelTreeNode(newChannelForm));
                node.Expand();
                newChannelForm.MdiParent = this;

                newChannelForm.Show();
                newChannelForm.AddLine("Joined: " + channel.Name);
            }
        }

        private void windowManagerTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;

            if (e.Node is ServerTreeNode)
            {
                ((ServerTreeNode)e.Node).ServerWindow.Focus();
            }
            else if (e.Node is ChannelTreeNode)
            {
                ((ChannelTreeNode)e.Node).ChannelWindow.Focus();
            }
            //TODO: PM and other windows
        }

        private void loggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogForm lf = new LogForm();
            lf.Show();
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            windowManagerTreeView.Dock = DockStyle.Left;
            splitter1.Dock = DockStyle.Left;
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            windowManagerTreeView.Dock = DockStyle.Right;
            splitter1.Dock = DockStyle.Right;
        }
    }
}