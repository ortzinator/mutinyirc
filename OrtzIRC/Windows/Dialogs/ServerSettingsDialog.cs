namespace OrtzIRC
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OrtzIRC.Common;

    public partial class ServerSettingsDialog : Form
    {
        //private ServerDataSet set = new ServerDataSet();
        public ServerSettingsDialog()
        {
            InitializeComponent();
        }

        private void ircSettingsTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.GetType() == typeof(ServerSettingsTreeNode))
            {
                var node = (ServerSettingsTreeNode)e.Node;
                ShowServerPane();
                serverUriTextBox.Text = node.Settings.Url;
                serverPortsTextBox.Text = node.Settings.Ports;
                serverDescriptionTextBox.Text = node.Settings.Description;
                autoConnectCheckBox.Checked = node.Settings.AutoConnect;
            }
            else if (e.Node.GetType() == typeof(NetworkSettingsTreeNode))
            {
                var node = (NetworkSettingsTreeNode)e.Node;
                ShowNetworkPane();
                networkNameTextBox.Text = node.Settings.Name;
            }
        }

        private void ircSettingsTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.GetType() == typeof(NetworkSettingsTreeNode))
            {
                ((NetworkSettingsTreeNode)e.Node).Settings.Name = e.Label;
                ircSettingsTree.SelectedNode = e.Node;
            }
            else if (e.Node.GetType() == typeof(ServerSettingsTreeNode))
            {
                // P90: Fix empty description (finally?)
                ((ServerSettingsTreeNode)e.Node).Settings.Description = e.Label ?? String.Empty;

                ircSettingsTree.SelectedNode = e.Node;
            }
        }

        private void serverPortsTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ircSettingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)ircSettingsTree.SelectedNode;
            node.Settings.Ports = serverPortsTextBox.Text;
        }

        private void serverUriTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ircSettingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)ircSettingsTree.SelectedNode;
            node.Settings.Url = serverUriTextBox.Text;
        }

        private void serverDescriptionTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ircSettingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)ircSettingsTree.SelectedNode;
            node.Settings.Description = serverDescriptionTextBox.Text;
            
            // P90: Fix empty server description
            node.Text = node.Settings.Description.Empty() ? node.Settings.Url : node.Settings.Description;
        }

        private void networkNameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ircSettingsTree.SelectedNode == null) return;
            var node = (NetworkSettingsTreeNode)ircSettingsTree.SelectedNode;
            node.Settings.Name = networkNameTextBox.Text;
            node.Text = node.Settings.Name;
        }

        private void autoConnectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ircSettingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)ircSettingsTree.SelectedNode;
            node.Settings.AutoConnect = autoConnectCheckBox.Checked;
        }

        protected override void OnLoad(EventArgs e)
        {
            foreach (var network in IrcSettingsManager.Instance.Networks)
            {
                var net = new NetworkSettingsTreeNode(network, networkContextMenuStrip);
                foreach (var server in network.Servers)
                {
                    var serverSettings = new ServerSettingsTreeNode(server, serverContextMenuStrip);
                    net.AddServerNode(serverSettings);
                }

                ircSettingsTree.Nodes.Add(net);
            }

            HideBothPanes();

            base.OnLoad(e);
        }

        private void ShowServerPane()
        {
            serverGroupBox.Location = new Point(153, 12);
            serverGroupBox.Enabled = true;
            serverGroupBox.Visible = true;

            networkGroupBox.Location = new Point(502, 12);
            networkGroupBox.Enabled = false;
            networkGroupBox.Visible = false;
        }

        private void ShowNetworkPane()
        {
            serverGroupBox.Location = new Point(502, 12);
            serverGroupBox.Enabled = false;
            serverGroupBox.Visible = false;

            networkGroupBox.Location = new Point(153, 12);
            networkGroupBox.Enabled = true;
            networkGroupBox.Visible = true;
        }

        private void HideBothPanes()
        {
            serverGroupBox.Enabled = false;
            serverGroupBox.Visible = false;

            networkGroupBox.Enabled = false;
            networkGroupBox.Visible = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            IrcSettingsManager.Instance.Save();
            Close();
        }

        private void addNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NetworkSettings net = IrcSettingsManager.Instance.AddNetwork("New Network");
            var node = new NetworkSettingsTreeNode(net, networkContextMenuStrip);
            ircSettingsTree.Nodes.Add(node);
            node.BeginEdit();
        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)((ToolStripMenuItem)sender).Owner;
            TreeView treeView = (TreeView)cms.SourceControl;
            TreeNode selectedNode = treeView.GetNodeAt(treeView.PointToClient(cms.Location));

            if (selectedNode.GetType() != typeof(NetworkSettingsTreeNode)) //Not sure if this would actually ever happen
                throw new InvalidOperationException();

            NetworkSettingsTreeNode netnode = (NetworkSettingsTreeNode)selectedNode;
            ServerSettings server = netnode.Settings.AddServer();
            server.Description = "New Server";
            var node = new ServerSettingsTreeNode(server, serverContextMenuStrip);
            netnode.AddServerNode(node);
            node.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)((ToolStripMenuItem)sender).Owner;
            TreeView treeView = (TreeView)cms.SourceControl;
            TreeNode selectedNode = treeView.GetNodeAt(treeView.PointToClient(cms.Location));

            IrcSettingsManager.Instance.RemoveNetwork(((NetworkSettingsTreeNode)selectedNode).Settings);
            ircSettingsTree.Nodes.Remove(selectedNode);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ContextMenuStrip cms = (ContextMenuStrip)((ToolStripMenuItem)sender).Owner;
            TreeView treeView = (TreeView)cms.SourceControl;
            TreeNode selectedNode = treeView.GetNodeAt(treeView.PointToClient(cms.Location));

            var netNode = (NetworkSettingsTreeNode)selectedNode.Parent;
            var serverNode = (ServerSettingsTreeNode)selectedNode;

            netNode.Settings.RemoveServer(serverNode.Settings);
            serverNode.Remove();
        }
    }
}