using System.Collections.Generic;

namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public partial class ServerDialog : Form
    {
        public ServerDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                List<NetworkSettings> networks = IRCSettingsManager.Instance.GetNetworks();

                if (networks.Count > 0)
                {
                    foreach (NetworkSettings network in networks)
                    {
                        var net = serverTree.Nodes.Add(network.Name);
                        foreach (var server in IRCSettingsManager.Instance.GetServers(network.Id))
                        {
                            net.Nodes.Add(new TreeNode { Text = server.Description });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("There are no networks. (Get a list somehow)"); //TODO
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            base.OnLoad(e);
        }
    }
}