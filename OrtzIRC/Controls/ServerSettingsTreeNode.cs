namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class ServerSettingsTreeNode : TreeNode
    {
        public ServerSettingsTreeNode(ServerSettings settings)
        {
            Text = settings.Description;
            Settings = settings;
        }

        public ServerSettings Settings { get; private set; }
    }
}
