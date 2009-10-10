namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class NetworkSettingsTreeNode : TreeNode
    {
        public NetworkSettingsTreeNode(NetworkSettings settings, ContextMenuStrip menu)
        {
            Text = settings.Name;
            Settings = settings;
            ContextMenuStrip = menu;
        }

        public NetworkSettings Settings { get; private set; }

        public void AddServerNode(ServerSettingsTreeNode node)
        {
            Nodes.Add(node);
        }
    }
}
