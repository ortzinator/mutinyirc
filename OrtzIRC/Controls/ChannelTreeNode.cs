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
            channelWindow.FormClosed += new FormClosedEventHandler(channelWindow_FormClosed);
        }

        private void channelWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Remove();
        }
    }
}