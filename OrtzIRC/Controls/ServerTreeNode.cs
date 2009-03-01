namespace OrtzIRC
{
    using System.Windows.Forms;

    public class ServerTreeNode : TreeNode
    {
        public ServerTreeNode(ServerForm serverWindow)
        {
            ServerWindow = serverWindow;
            Text = serverWindow.Server.Description;
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
    }
}
