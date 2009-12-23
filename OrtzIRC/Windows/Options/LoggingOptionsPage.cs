namespace OrtzIRC
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using OrtzIRC.Common;
    using OrtzIRC.Properties;
    using System.Drawing;

    public partial class LoggingOptionsPage : OptionsPage
    {
        private static DirectoryInfo RunningDir = new DirectoryInfo(Environment.CurrentDirectory);
        private static DateTime Time;
        private bool first = true;

        public LoggingOptionsPage()
        {
            InitializeComponent();
        }

        public override void OnSetActive()
        {
            if (!first) return;

            ToggleControls(false);

            // Figure out where our logs are
            DirectoryInfo LogDir = new DirectoryInfo(Environment.CurrentDirectory + "\\logs");

            // Store the time so we can show an example of the timestamp
            Time = DateTime.Now;
            timestampExampleTextLabel.Text = Time.ToString(timestampFormatTextbox.Text);

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
            first = false;
        }

        // Allows us to turn off the controls depending on context
        private void ToggleControls(bool enable)
        {
            openLogButton.Enabled = enable;
            viewLogButton.Enabled = enable;
            deleteLogButton.Enabled = enable;
            deleteAllLogButton.Enabled = enable;
        }

        public override void OnApply()
        {
            // Clear the tree on close so that we refill it with appropriate files on loading
            // (Will not show deleted logs)

            logFilesTreeView.Nodes.Clear();

            TextLoggerManager.LoggerActive = activateLoggerCheckBox.Checked;
            TextLoggerManager.AddTimestamp = addTimestampCheckbox.Checked;
            TextLoggerManager.TimeFormat = timestampFormatTextbox.Text;
        }

        public override string Text
        {
            get
            {
                return "Logging";
            }
        }

        public override Image Image
        {
            get
            {
                return Resources.Icons.Notebook;
            }
        }

        private void openLogButton_Click(object sender, EventArgs e)
        {
            Process.Start(logFilesTreeView.SelectedNode.Name);
        }

        private void viewLogButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException(
                @"Eventually this should be a notepad-like
                editor included with OtrzIRC, perhaps specially
                made for log parsing.");
        }

        private void deleteLogButton_Click(object sender, EventArgs e)
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

        private void deleteAllLogButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("WARNING: This will delete ALL logs files from the logs directory. This cannot be undone. Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dr != DialogResult.No)
                return;

            DirectoryInfo di = new DirectoryInfo(RunningDir.FullName + "\\logs");

            if (di.Exists)
            {
                di.Delete(true);
            }

            logFilesTreeView.Nodes.Clear();
        }

        private void logFilesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
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

        private void timestampFormatTextbox_TextChanged(object sender, EventArgs e)
        {
            timestampExampleTextLabel.Text = Time.ToString(timestampFormatTextbox.Text);
        }
    }
}
