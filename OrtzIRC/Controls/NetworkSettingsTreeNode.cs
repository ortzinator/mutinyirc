namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class NetworkSettingsTreeNode : TreeNode
    {
        public NetworkSettingsTreeNode(NetworkSettings settings)
        {
            Settings = settings;
            Text = settings.Name;
        }

        public NetworkSettings Settings { get; private set; }

        public void AddServerNode(ServerSettingsTreeNode node)
        {
            Nodes.Add(node);
        }
    }
}
