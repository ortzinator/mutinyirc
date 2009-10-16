namespace OrtzIRC
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;
    using System.Diagnostics;

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

            Settings.Default.UserPluginDirectory = Path.Combine(Environment.CurrentDirectory, @"Plugins");
            Settings.Default.Save();

            if (!Directory.Exists(Settings.Default.UserPluginDirectory))
                Directory.CreateDirectory(Settings.Default.UserPluginDirectory);

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Server newServer = ServerManager.Instance.Create("irc.randomirc.com", "RandomIRC", 6667, false);
                CreateServerForm(newServer);
                newServer.Connect();
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
                windowTreeView.AddServerNode(new ServerTreeNode(newServerForm));
                newServerForm.MdiParent = this;
            });

            newServerForm.Text = server.Description;
            newServerForm.Show();

            return newServerForm;
        }

        /// <summary>
        /// Creates and displays a ChannelForm and adds a node to the WindowManagerTreeView under the appropriate server node
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public ChannelForm CreateChannelForm(Channel channel)
        {
            ChannelDelegate del = CreateChannelFormOnUiThread;
            return (ChannelForm)Invoke(del, channel);
        }

        private delegate ChannelForm ChannelDelegate(Channel channel);

        private ChannelForm CreateChannelFormOnUiThread(Channel channel)
        {
            ChannelForm newChannelForm = new ChannelForm(channel);
            ServerTreeNode node = windowTreeView.GetServerNode(channel.Server);

            if (node == null)
                throw new Exception("ServerTreeNode doesn't exist!"); //hack
            Invoke((MethodInvoker)delegate
            {
                node.AddChannelNode(new ChannelTreeNode(newChannelForm));
                node.Expand();
                newChannelForm.MdiParent = this;
            });

            newChannelForm.Show();
            newChannelForm.AddLine("Joined: " + channel.Name);
            //TODO: display topic

            return newChannelForm;
        }

        private void windowTreeView_AfterSelect(object sender, TreeViewEventArgs e)
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
    }
}