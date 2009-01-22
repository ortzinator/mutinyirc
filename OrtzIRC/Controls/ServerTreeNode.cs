namespace OrtzIRC.Controls
{
    using System.Windows.Forms;

    public class ServerTreeNode : TreeNode
    {
        public ServerForm ServerWindow { get; private set; }

        public ServerTreeNode(ServerForm serverWindow)
        {
            this.ServerWindow = serverWindow;
            this.Text = serverWindow.Server.Description;
        }

        public void AddChannelNode(ChannelTreeNode channelNode)
        {
            this.Nodes.Add(channelNode);
        }
    }
}
