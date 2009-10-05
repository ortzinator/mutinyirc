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

            settingsTree.NodeMouseClick += settingsTree_NodeMouseClick;
            networkNameTextBox.Validating += networkNameTextBox_Validating;

            serverDescriptionTextBox.Validating += serverDescriptionTextBox_Validating;
            serverUriTextBox.Validating += serverUriTextBox_Validating;
            serverPortsTextBox.Validating += serverPortsTextBox_Validating;
        }

        private void serverPortsTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)settingsTree.SelectedNode;
            node.Settings.Ports = serverPortsTextBox.Text;
        }

        private void serverUriTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)settingsTree.SelectedNode;
            node.Settings.Url = serverUriTextBox.Text;
        }

        private void serverDescriptionTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)settingsTree.SelectedNode;
            node.Settings.Description = serverDescriptionTextBox.Text;
            node.Text = node.Settings.Description;
        }

        private void networkNameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (NetworkSettingsTreeNode)settingsTree.SelectedNode;
            node.Settings.Name = networkNameTextBox.Text;
            node.Text = node.Settings.Name;
        }

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.GetType() == typeof(ServerSettingsTreeNode))
            {
                var node = (ServerSettingsTreeNode)e.Node;
                ShowServerPane();
                serverUriTextBox.Text = node.Settings.Url;
                serverPortsTextBox.Text = node.Settings.Ports;
                serverDescriptionTextBox.Text = node.Settings.Description;
            }
            else if (e.Node.GetType() == typeof(NetworkSettingsTreeNode))
            {
                var node = (NetworkSettingsTreeNode)e.Node;
                ShowNetworkPane();
                networkNameTextBox.Text = node.Settings.Name;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            
            foreach (var network in IRCSettingsManager.Instance.Networks)
            {
                var net = new NetworkSettingsTreeNode(network);
                foreach (var server in network.Servers)
                {
                    net.AddServerNode(new ServerSettingsTreeNode(server));
                }
                settingsTree.Nodes.Add(net);
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
            IRCSettingsManager.Instance.Save();
            Close();
        }
    }
}