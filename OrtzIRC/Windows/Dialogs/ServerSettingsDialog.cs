namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.Common.ServerDataSetTableAdapters;
    using System.Collections.Generic;
    using System.Drawing;

    public partial class ServerSettingsDialog : Form
    {
        public ServerSettingsDialog()
        {
            InitializeComponent();

            settingsTree.NodeMouseClick += settingsTree_NodeMouseClick;
        }

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.GetType() == typeof(ServerSettingsTreeNode))
            {
                var node = (ServerSettingsTreeNode)e.Node;
                ShowServerPane();
                serverUriTextBox.Text = node.Settings.Uri;
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
            Console.Write(System.Data.Common.DbProviderFactories.GetFactory("System.Data.SQLite").ToString());

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
    }
}