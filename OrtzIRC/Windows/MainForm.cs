namespace OrtzIRC
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;
    using OrtzIRC.Resources;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Form focusedChild;

        protected override void OnLoad(EventArgs e)
        {
            Settings.Default.SettingsSaving += Default_SettingsSaving;
            ServerManager.Instance.ServerAdded += Instance_ServerCreated;

            windowManagerTreeView.AfterSelect += windowManagerTreeView_AfterSelect;

            serversMenuItem.Visible = false;

#if DEBUG
            serversMenuItem.Visible = true;
#endif

            LoadSettings();

            if (MessageBox.Show("Do you wish to connect?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) //hack
            {
                foreach (ServerSettings server in IrcSettingsManager.Instance.GetAutoConnectServers())
                {
                    if (server.Nick == null)
                        server.Nick = Settings.Default.FirstNick;

                    Server newServer = ServerManager.Instance.Create(new ConnectionArgs(server.Nick, server.Url, server.Ssl));
                    newServer.Connect();
                }
            }

            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "plugins")))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "plugins"));

            PluginManager.LoadPlugins(Path.Combine(Environment.CurrentDirectory, "plugins"));
            PluginManager.LoadPlugins(Settings.Default.UserPluginDirectory);
            RandomMessages.Load();

            base.OnLoad(e);
        }

        private void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (Settings.Default.UserPluginDirectory == String.Empty)
            {
                Settings.Default.UserPluginDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"OrtzIRC/Plugins");
                Settings.Default.Save();
            }

            if (!Directory.Exists(Settings.Default.UserPluginDirectory))
                Directory.CreateDirectory(Settings.Default.UserPluginDirectory);

            TextLoggerManager.LoggerActive = Settings.Default.LoggerActivated;
            TextLoggerManager.AddTimestamp = Settings.Default.LoggerTimestampsActivated;
            TextLoggerManager.TimeFormat = Settings.Default.LoggerTimestampFormat;
        }

        private void Instance_ServerCreated(object sender, ServerEventArgs e)
        {
            CreateServerForm(e.Server);
            e.Server.PrivateMessageSessionAdded += Server_PrivateMessageSessionAdded;
        }

        private void Server_PrivateMessageSessionAdded(object sender, PrivateMessageSessionEventArgs e)
        {
            CreatePmForm(e.PrivateMessageSession);
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void serversMenuItem_Click(object sender, EventArgs e)
        {
            var servers = new ServerSettingsDialog();
            servers.ShowDialog();
        }

        public void CreatePmForm(PrivateMessageSession pm)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<PrivateMessageSession>(CreatePmForm), pm);
            }
            else
            {
                var newPmForm = new PrivateMessageForm { PMSession = pm };

                ServerTreeNode node = windowManagerTreeView.GetServerNode(pm.Server);

                node.AddPmNode(new PmTreeNode(newPmForm));
                node.Expand();
                newPmForm.MdiParent = this;
                newPmForm.Show();
                newPmForm.Enter += newForm_Enter;
            }
        }

        /// <summary>
        /// Creates and displays a ServerForm and adds a node to the WindowManagerTreeView
        /// </summary>
        /// <param name="server"></param>
        public void CreateServerForm(Server server)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Server>(CreateServerForm), server);
            }
            else
            {
                var newServerForm = new ServerForm { Server = server };

                windowManagerTreeView.AddServerNode(new ServerTreeNode(newServerForm));
                newServerForm.MdiParent = this;

                newServerForm.Text = server.Url;
                newServerForm.Show();
                newServerForm.Enter += newForm_Enter;
            }
        }

        /// <summary>
        /// Creates and displays a ChannelForm and adds a node to the WindowManagerTreeView under the appropriate server node
        /// </summary>
        /// <param name="channel"></param>
        public void CreateChannelForm(Channel channel)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Channel>(CreateChannelForm), new object[] { channel });
            }
            else
            {
                var newChannelForm = new ChannelForm(channel);
                ServerTreeNode node = windowManagerTreeView.GetServerNode(channel.Server);

                node.AddChannelNode(new ChannelTreeNode(newChannelForm));
                node.Expand();
                newChannelForm.MdiParent = this;

                newChannelForm.Show();
                newChannelForm.AddLine("Joined: " + channel.Name);

                newChannelForm.Enter += newForm_Enter;
            }
        }

        private void newForm_Enter(object sender, EventArgs e)
        {
            focusedChild = sender as Form;

            if (focusedChild == null) return;

            var server = focusedChild as ServerForm;
            if (server != null)
            {
                connectToolStripMenuItem.Visible = !server.Server.IsConnected;
                disconnectToolStripMenuItem.Visible = server.Server.IsConnected;
            }

            var channel = focusedChild as ChannelForm;
            if (channel != null)
            {
                connectToolStripMenuItem.Visible = !channel.Channel.Server.IsConnected;
                disconnectToolStripMenuItem.Visible = channel.Channel.Server.IsConnected;
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
            //TODO: Create an abstract Window property to remove redundancy
        }

        private void loggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lf = new LogForm();
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainForm Closing: " + e.CloseReason);

            bool exit = true;
            if (ServerManager.Instance.AnyConnected())
                exit = ConfirmExit();

            if (exit)
            {
                RandomMessages.Save();
                IrcSettingsManager.Instance.Save();
                ServerManager.Instance.DisconnectAll();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private bool ConfirmExit()
        {
            DialogResult dr = MessageBox.Show(CommonStrings.MainExitPrompt, CommonStrings.MainExitPromptCaption, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            return (dr == DialogResult.Yes);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OptionsDialog();
            dlg.Pages.Add(new GeneralOptionsPage());
            dlg.Pages.Add(new LoggingOptionsPage());
            DialogResult result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                Settings.Default.Save();
            }
            else
            {
                Settings.Default.Reload();
            }
        }

        public Form CurrentlyFocusedWindow()
        {
            return focusedChild;
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form window = CurrentlyFocusedWindow();

            if (window == null) return;

            var server = window as ServerForm;
            if (server != null)
            {
                server.Server.Disconnect();
            }

            var channel = window as ChannelForm;
            if (channel != null)
            {
                channel.Channel.Server.Disconnect();
            }
        }

        private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var db = new InputDialog("Connect to server...", "Server Url:", "Connect");

            if (db.ShowDialog() == DialogResult.OK)
            {
                var svr = ServerManager.Instance.Create(new ConnectionArgs(Settings.Default.FirstNick, db.Input, false));
                svr.Connect();
            }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form window = CurrentlyFocusedWindow();

            if (window == null) return;

            var server = window as ServerForm;
            if (server != null)
            {
                server.Server.Connect();
            }

            var channel = window as ChannelForm;
            if (channel != null)
            {
                channel.Channel.Server.Connect();
            }
        }
    }
}