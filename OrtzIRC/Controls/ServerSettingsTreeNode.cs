namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class ServerSettingsTreeNode : TreeNode
    {
        public ServerSettingsTreeNode(ServerSettings settings)
        {
            Settings = settings;
            Text = settings.Description;
        }

        public ServerSettings Settings { get; private set; }
    }
}
