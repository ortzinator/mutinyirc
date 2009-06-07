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
            var networks = IRCSettingsManager.Instance.GetNetworks();

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
                MessageBox.Show("There are no networks. (Get a list somehow)");
            }
            
            base.OnLoad(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Server temp = new Server("irc.gamesurge.net", "Hello new server", 6667);

            //Settings.Default.ServerList.Add(temp);
        }
    }
}