namespace OrtzIRC
{
    using System.Windows.Forms;

    public class PmTreeNode : TreeNode
    {
        public PmTreeNode(PrivateMessageForm pmWindow)
        {
            PmWindow = pmWindow;
            Text = pmWindow.PMSession.User.Nick;
            pmWindow.FormClosed += pmWindow_FormClosed;

            ContextMenuStrip = new PmNodeContextMenu();
            ((PmNodeContextMenu)ContextMenuStrip).CloseClick += delegate { PmWindow.Close(); };
        }

        public PrivateMessageForm PmWindow { get; private set; }

        private void pmWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Remove();
        }
    }
}