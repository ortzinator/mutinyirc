namespace OrtzIRC
{
    using System.Windows.Forms;
    using System.Collections;
    using OrtzIRC.Common;

    public partial class WindowManagerTreeView : TreeView
    {
        public WindowManagerTreeView()
        {
            InitializeComponent();
        }

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

        public void AddServerNode(ServerTreeNode serverNode)
        {
            Nodes.Add(serverNode);
        }

        public ServerTreeNode GetServerNode(Server server)
        {
            foreach (ServerTreeNode serverNode in ServerNodes)
            {
                if (serverNode.ServerWindow.Server == server)
                    return serverNode;
            }
            return null;
        }
    }
}
