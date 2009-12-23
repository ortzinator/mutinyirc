namespace OrtzIRC
{
    using System.Windows.Forms;

    public class ChannelTreeNode : TreeNode
    {
        public ChannelTreeNode(ChannelForm channelWindow)
        {
            ChannelWindow = channelWindow;
            Text = channelWindow.Channel.Info.Name;
            channelWindow.FormClosed += channelWindow_FormClosed;

            ContextMenuStrip = new ChannelNodeContextMenu();
            ((ChannelNodeContextMenu)ContextMenuStrip).LeaveClick += delegate { ChannelWindow.Close(); };
        }

        public ChannelForm ChannelWindow { get; private set; }

        private void channelWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Remove();
        }
    }
}