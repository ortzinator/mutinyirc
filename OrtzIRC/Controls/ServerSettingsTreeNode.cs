namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class ServerSettingsTreeNode : TreeNode
    {
        public ServerSettingsTreeNode(IrcSettingsDataSet.ServersRow row)
        {
            Text = row.Description;
            Row = row;
        }

        public IrcSettingsDataSet.ServersRow Row { get; private set; }
    }
}
