namespace OrtzIRC.Controls
{
    using System.Windows.Forms;
    using System.Collections;
    using OrtzIRC.Common;

    public partial class WindowManagerTreeView : TreeView
    {
        public IEnumerable ServerNodes
        {
            get
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].GetType() == typeof(ServerTreeNode))
                        yield return Nodes[i];
                }
            }
        }

        public WindowManagerTreeView()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public void AddServerNode(ServerTreeNode serverNode)
        {
            this.Nodes.Add(serverNode);
        }

        public ServerTreeNode GetServerNode(Server server)
        {
            foreach (ServerTreeNode serverNode in this.ServerNodes)
            {
                if (serverNode.ServerWindow.Server == server)
                    return serverNode;
            }
            return null;
        }
    }
}
