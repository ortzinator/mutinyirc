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
