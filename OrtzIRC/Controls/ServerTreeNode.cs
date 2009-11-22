namespace OrtzIRC
{
    using System.Windows.Forms;

    public class ServerTreeNode : TreeNode
    {
        public ServerTreeNode(ServerForm serverWindow)
        {
            ServerWindow = serverWindow;
            // P90: Fix empty server description
            Text = serverWindow.Server.Description.Length == 0 ? serverWindow.Server.URL : serverWindow.Server.Description;
            serverWindow.FormClosed += serverWindow_FormClosed;

            ContextMenuStrip = new ServerNodeContextMenu();
            ((ServerNodeContextMenu)ContextMenuStrip).DisconnectClick += delegate { ServerWindow.Close(); };
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
