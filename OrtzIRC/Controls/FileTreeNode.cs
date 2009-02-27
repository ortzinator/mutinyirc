namespace OrtzIRC
{
    using System.Windows.Forms;

    // This class is there to make treenode management easier
    // By differencing between directories and files, it's a lot easier to
    // work with the button's operators.
    class FileTreeNode : TreeNode
    {
        // Is file meant for text only display?
        public bool IsDummy { get; set; }

        public bool IsFile { get; set; }

        private string fileName;

        public string Path { get; set; }

        public FileTreeNode(bool isFile, string name, string path)
        {
            IsFile = isFile;
            IsDummy = false;
            fileName = name;
            Path = path;

            // Set base attributes
            Name = Path;
            Text = name;
        }

        // Dummies are used for displaying text in the node
        public FileTreeNode(bool isDummy, string text)
        {
            IsDummy = true;
            Text = text;
        }
    }
}
