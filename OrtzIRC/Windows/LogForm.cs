namespace OrtzIRC
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    // Logs display and management class
    // Made by Guillaume 'gparent' Parent (gparent@gmail.com) 2009
    public partial class LogForm : Form
    {
        private static DirectoryInfo RunningDir = new DirectoryInfo(Environment.CurrentDirectory);

        public LogForm()
        {
            InitializeComponent();
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            ToggleControls(false);

            // Figure out where our logs are
            DirectoryInfo LogDir = new DirectoryInfo(Environment.CurrentDirectory + "\\logs");

            if (LogDir.Exists)
            {
                // Get the network subdirectories
                DirectoryInfo[] LogSubDirs = LogDir.GetDirectories();

                if (LogSubDirs.Length != 0)
                {
                    // For each network, add channel logs and private messages
                    foreach (DirectoryInfo di in LogSubDirs)
                    {
                        FileNode dirNode = new FileNode(false, di.Name + '\\', di.FullName);

                        // Get the channel/pmsg logs
                        FileInfo[] SubFiles = di.GetFiles();

                        foreach (FileInfo file in SubFiles)
                        {
                            FileNode fileNode = new FileNode(true, file.Name, file.FullName);

                            dirNode.Nodes.Add(fileNode);
                        }

                        // Add network node to list
                        LogTVLogfiles.Nodes.Add(dirNode);
                    }
                }
                else // Found no network subdirs
                {
                    LogTVLogfiles.Nodes.Add(new FileNode(true, "No logs found"));
                }
            }
            else // Found no logs directory
            {
                LogTVLogfiles.Nodes.Add(new FileNode(true, "Log directory doesn't exist."));
            }
        }

        // Allows us to turn off the controls depending on context
        private void ToggleControls(bool enable)
        {
            LogBTNOpen.Enabled = enable;
            LogBTNView.Enabled = enable;
            LogBTNDelete.Enabled = enable;
            LogBTNDeleteAll.Enabled = enable;
        }

        // Clear the tree on close so that we refill it with appropriate files on loading
        // (Will not show deleted logs)
        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogTVLogfiles.Nodes.Clear();
        }

        // Opens a log with the default text editor
        private void LogBTNOpen_Click(object sender, EventArgs e)
        {
            Process.Start(LogTVLogfiles.SelectedNode.Name);
        }

        // This should open the log with our own log browser
        private void LogBTNView_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException(
                @"Eventually this should be a notepad-like
                editor included with OtrzIRC, perhaps specially
                made for log parsing.");
        }

        // Deletes a single log file or a directory (NYI)
        private void LogBTNDelete_Click(object sender, EventArgs e)
        {
            FileNode fn = (FileNode)LogTVLogfiles.SelectedNode;

            DialogResult dr = MessageBox.Show("Do you really want to delete " + fn.Text + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dr == DialogResult.No)
                return;


            if (fn.isFile)
            {
                FileInfo fi = new FileInfo(fn.Path);

                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(fn.Path);

                if (di.Exists)
                {
                    di.Delete(true);
                }
            }

            LogTVLogfiles.Nodes.Remove(LogTVLogfiles.SelectedNode);
        }

        // Clears the entire log directory
        private void LogBTNDeleteAll_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("WARNING: This will delete ALL logs files from the logs directory. This cannot be undone. Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dr == DialogResult.Yes)
            {
                DirectoryInfo di = new DirectoryInfo(RunningDir.FullName + "\\logs");

                if (di.Exists)
                {
                    di.Delete(true);
                }

                LogTVLogfiles.Nodes.Clear();
            }
        }

        // Updates the controls depending on whether or not a file is selected or not
        private void LogTVLogfiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FileNode fn = (FileNode)e.Node;

            if (!fn.isDummy && fn.isFile)
            {
                ToggleControls(true);
            }
            else
            {
                ToggleControls(false);
            }
        }
    }

    // This class is there to make treenode management easier
    // By differencing between directories and files, it's a lot easier to
    // work with the button's operators.
    class FileNode : TreeNode
    {
        // Is file meant for text only display?
        public bool isDummy;


        public bool isFile;


        string fileName;
        public string Path; // Even if path == name, it's not very explicit.

        public FileNode(bool isFile, string name, string Path)
            : base()
        {
            this.isFile = isFile;
            isDummy = false;
            fileName = name;
            this.Path = Path;

            // Set base attributes
            base.Name = Path;
            base.Text = name;
        }

        // Dummies are used for displaying text in the node
        public FileNode(bool isDummy, string text)
            : base()
        {
            isDummy = true;
            base.Text = text;
        }
    }
}
