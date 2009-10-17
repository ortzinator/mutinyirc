namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class ServerSettingsTreeNode : TreeNode
    {
        public ServerSettingsTreeNode(ServerSettings settings, ContextMenuStrip menu)
        {
            // P90: Fix empty server description
            Text = settings.Description.Length == 0 ? settings.Url : settings.Description;
            Settings = settings;
            ContextMenuStrip = menu;
        }

        public ServerSettings Settings { get; private set; }
    }
}
