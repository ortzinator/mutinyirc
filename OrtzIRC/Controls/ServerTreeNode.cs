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
            serverWindow.FormClosed += new FormClosedEventHandler(serverWindow_FormClosed);
        }

        private void serverWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Remove();
        }

        public void AddChannelNode(ChannelTreeNode channelNode)
        {
            this.Nodes.Add(channelNode);
        }
    }
}
