namespace OrtzIRC
{
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class ServerSettingsTreeNode : TreeNode
    {
        public ServerSettingsTreeNode(ServerSettings settings, ContextMenuStrip menu)
        {
            Text = settings.Description.Empty() ? settings.Url : settings.Description;
            Settings = settings;
            ContextMenuStrip = menu;
        }

        public ServerSettings Settings { get; private set; }
    }
}
