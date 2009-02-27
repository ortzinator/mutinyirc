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
                        FileTreeNode dirNode = new FileTreeNode(false, di.Name + '\\', di.FullName);

                        // Get the channel/pmsg logs
                        FileInfo[] SubFiles = di.GetFiles();

                        foreach (FileInfo file in SubFiles)
                        {
                            FileTreeNode fileNode = new FileTreeNode(true, file.Name, file.FullName);

                            dirNode.Nodes.Add(fileNode);
                        }

                        // Add network node to list
                        logFilesTreeView.Nodes.Add(dirNode);
                    }
                }
                else // Found no network subdirs
                {
                    logFilesTreeView.Nodes.Add(new FileTreeNode(true, "No logs found"));
                }
            }
            else // Found no logs directory
            {
                logFilesTreeView.Nodes.Add(new FileTreeNode(true, "Log directory doesn't exist."));
            }
        }

        // Allows us to turn off the controls depending on context
        private void ToggleControls(bool enable)
        {
            openLogButton.Enabled = enable;
            viewLogButton.Enabled = enable;
            deleteLogButton.Enabled = enable;
            deleteAllLogButton.Enabled = enable;
        }

        // Clear the tree on close so that we refill it with appropriate files on loading
        // (Will not show deleted logs)
        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            logFilesTreeView.Nodes.Clear();
        }

        // Opens a log with the default text editor
        private void LogBTNOpen_Click(object sender, EventArgs e)
        {
            Process.Start(logFilesTreeView.SelectedNode.Name);
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
            FileTreeNode fn = (FileTreeNode)logFilesTreeView.SelectedNode;

            DialogResult dr = MessageBox.Show("Do you really want to delete " + fn.Text + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dr == DialogResult.No)
                return;


            if (fn.IsFile)
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

            logFilesTreeView.Nodes.Remove(logFilesTreeView.SelectedNode);
        }

        // Clears the entire log directory
        private void LogBTNDeleteAll_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("WARNING: This will delete ALL logs files from the logs directory. This cannot be undone. Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dr != DialogResult.Yes) return;

            DirectoryInfo di = new DirectoryInfo(RunningDir.FullName + "\\logs");

            if (di.Exists)
            {
                di.Delete(true);
            }

            logFilesTreeView.Nodes.Clear();
        }

        // Updates the controls depending on whether or not a file is selected or not
        private void LogTVLogfiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FileTreeNode fn = (FileTreeNode)e.Node;

            if (!fn.IsDummy && fn.IsFile)
            {
                ToggleControls(true);
            }
            else
            {
                ToggleControls(false);
            }
        }
    }
}
