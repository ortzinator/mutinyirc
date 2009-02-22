using OrtzIRC.Windows;
namespace OrtzIRC
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.Controls;
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
            Settings.Default.UserPluginDirectory = Path.Combine(Environment.CurrentDirectory, @"Plugins");
            Settings.Default.Save();

            if (!Directory.Exists(Settings.Default.UserPluginDirectory))
                Directory.CreateDirectory(Settings.Default.UserPluginDirectory);

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Server newServer = ServerManager.Instance.Create("irc.randomirc.com", "RandomIRC", 6667, false);
                this.CreateServerForm(newServer);
                newServer.Connect();
            }

            PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);

            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newServerMenuItem_Click(object sender, EventArgs e)
        {
            Dialogs.ServerDialog servers = new Dialogs.ServerDialog();
            servers.ShowDialog();
        }

        /// <summary>
        /// Creates and displays a ServerForm and adds a node to the WindowManagerTreeView
        /// </summary>
        /// <param name="server"></param>
        /// <returns>The ServerForm that was created</returns>
        public ServerForm CreateServerForm(Server server)
        {
            ServerForm newServerForm = new ServerForm() { Server = server };

            this.Invoke((MethodInvoker)delegate
            {
                windowTreeView.AddServerNode(new OrtzIRC.Controls.ServerTreeNode(newServerForm));
                newServerForm.MdiParent = this;
            });

            newServerForm.Text = server.Description;
            newServerForm.Show();
            //TODO: display topic

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
            return (ChannelForm)this.Invoke(del, channel);
        }

        private delegate ChannelForm ChannelDelegate(Channel channel);

        private ChannelForm CreateChannelFormOnUiThread(Channel channel)
        {
            ChannelForm newChannelForm = new ChannelForm(channel);
            ServerTreeNode node = windowTreeView.GetServerNode(channel.Server);

            if (node == null)
                throw new Exception("ServerTreeNode doesn't exist!"); //hack

            this.Invoke((MethodInvoker)delegate
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
            if (e.Action != TreeViewAction.Unknown)
            {
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
        }

        private void loggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogForm lf = new LogForm();
            lf.Show();
        }
    }
}