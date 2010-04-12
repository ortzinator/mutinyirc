using System;

namespace OrtzIRC
{
    using System.Windows.Forms;

    public class ServerTreeNode : TreeNode
    {
        public ServerTreeNode(ServerForm serverWindow)
        {
            ServerWindow = serverWindow;
            Text = serverWindow.Server.Url;
            serverWindow.FormClosed += serverWindow_FormClosed;

            ContextMenuStrip = new ServerNodeContextMenu();
            ((ServerNodeContextMenu)ContextMenuStrip).DisconnectClick += delegate { ServerWindow.Close(); };
            serverWindow.Server.Registered += Server_Registered;
        }

        private void Server_Registered(object sender, System.EventArgs e)
        {
            if (TreeView.InvokeRequired)
                TreeView.Invoke(new Action(DoRegister));
            else
                DoRegister();
        }

        private void DoRegister()
        {
            string network = ServerWindow.Server.Connection.ServerProperties["Network"];
            var networkSettings = IrcSettingsManager.Instance.GetNetwork(ServerWindow.Server);

            if (network == String.Empty)
            {
                if (networkSettings != null)
                {
                    network = networkSettings.Name;
                }
            }
            else
            {
                networkSettings.Name = network;
            }

            Text = network;
        }

        public ServerForm ServerWindow { get; private set; }

        private void serverWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Remove();
        }

        public void AddChannelNode(ChannelTreeNode channelNode)
        {
            Nodes.Add(channelNode);
        }

        public void AddPmNode(PmTreeNode pmNode)
        {
            Nodes.Add(pmNode);
        }
    }
}
