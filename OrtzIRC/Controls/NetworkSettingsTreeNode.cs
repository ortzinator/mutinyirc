namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class NetworkSettingsTreeNode : TreeNode
    {
        public NetworkSettingsTreeNode(IrcSettingsDataSet.NetworksRow row)
        {
            Text = row.Name;
            Row = row;
        }

        public IrcSettingsDataSet.NetworksRow Row { get; private set; }

        public void AddServerNode(ServerSettingsTreeNode node)
        {
            Nodes.Add(node);
        }
    }
}
