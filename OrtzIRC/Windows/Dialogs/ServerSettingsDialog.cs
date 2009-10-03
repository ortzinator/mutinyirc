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
            node.Row.Ports = serverPortsTextBox.Text;
        }

        private void serverUriTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)settingsTree.SelectedNode;
            node.Row.Url = serverUriTextBox.Text;
        }

        private void serverDescriptionTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (ServerSettingsTreeNode)settingsTree.SelectedNode;
            node.Row.Description = serverDescriptionTextBox.Text;
            node.Text = node.Row.Description;
        }

        private void networkNameTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsTree.SelectedNode == null) return;
            var node = (NetworkSettingsTreeNode)settingsTree.SelectedNode;
            node.Row.Name = networkNameTextBox.Text;
            node.Text = node.Row.Name;
        }

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.GetType() == typeof(ServerSettingsTreeNode))
            {
                var node = (ServerSettingsTreeNode)e.Node;
                ShowServerPane();
                serverUriTextBox.Text = node.Row.Url;
                serverPortsTextBox.Text = node.Row.Ports;
                serverDescriptionTextBox.Text = node.Row.Description;
            }
            else if (e.Node.GetType() == typeof(NetworkSettingsTreeNode))
            {
                var node = (NetworkSettingsTreeNode)e.Node;
                ShowNetworkPane();
                networkNameTextBox.Text = node.Row.Name;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                networkDataAdapter.Fill(ircSettingsDataSet1);
                //var set = new ServerDataSet();
                serverDataAdapter.Fill(ircSettingsDataSet1);

                foreach (var row in ircSettingsDataSet1.Servers)
                {
                    MessageBox.Show(row.Url);
                }
                
                foreach (IrcSettingsDataSet.NetworksRow row in ircSettingsDataSet1.Networks)
                {
                    var net = new NetworkSettingsTreeNode(row);
                    foreach (var serversRow in net.Row.GetServersRows())
                    {
                        net.AddServerNode(new ServerSettingsTreeNode(serversRow));
                    }
                    settingsTree.Nodes.Add(net);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ircSettingsDataSet1.Servers.Rows[1].RowError);
                throw;
            }

            HideBothPanes();

            base.OnLoad(e);
        }

        void networkDataAdapter_FillError(object sender, System.Data.FillErrorEventArgs e)
        {
            throw new NotImplementedException();
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
            networkDataAdapter.Update(ircSettingsDataSet1);
            Close();
        }
    }
}