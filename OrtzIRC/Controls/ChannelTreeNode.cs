namespace OrtzIRC.Controls
{
    using System.Windows.Forms;

    public class ChannelTreeNode : TreeNode
    {
        public ChannelForm ChannelWindow { get; private set; }

        public ChannelTreeNode(ChannelForm channelWindow)
        {
            ChannelWindow = channelWindow;
            this.Text = channelWindow.Channel.Name;
        }
    }
}