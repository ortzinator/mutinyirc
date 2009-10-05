namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class NetworkSettingsTreeNode : TreeNode
    {
        public NetworkSettingsTreeNode(NetworkSettings settings)
        {
            Text = settings.Name;
            Settings = settings;
        }

        public NetworkSettings Settings { get; private set; }

        public void AddServerNode(ServerSettingsTreeNode node)
        {
            Nodes.Add(node);
        }
    }
}
