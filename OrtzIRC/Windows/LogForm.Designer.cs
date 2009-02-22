namespace OrtzIRC
{
    partial class LogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.openLogButton = new System.Windows.Forms.Button();
            this.viewLogButton = new System.Windows.Forms.Button();
            this.deleteLogButton = new System.Windows.Forms.Button();
            this.LogOpenTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.logFilesTreeView = new System.Windows.Forms.TreeView();
            this.deleteAllLogButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 419);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(321, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Note: Logging can be enabled or disabled within the options dialog";
            // 
            // openLogButton
            // 
            this.openLogButton.Location = new System.Drawing.Point(295, 81);
            this.openLogButton.Name = "openLogButton";
            this.openLogButton.Size = new System.Drawing.Size(135, 27);
            this.openLogButton.TabIndex = 1;
            this.openLogButton.Text = "Open";
            this.openLogButton.UseVisualStyleBackColor = true;
            this.openLogButton.Click += new System.EventHandler(this.LogBTNOpen_Click);
            // 
            // viewLogButton
            // 
            this.viewLogButton.Location = new System.Drawing.Point(295, 114);
            this.viewLogButton.Name = "viewLogButton";
            this.viewLogButton.Size = new System.Drawing.Size(135, 27);
            this.viewLogButton.TabIndex = 2;
            this.viewLogButton.Text = "View";
            this.viewLogButton.UseVisualStyleBackColor = true;
            this.viewLogButton.Click += new System.EventHandler(this.LogBTNView_Click);
            // 
            // deleteLogButton
            // 
            this.deleteLogButton.Location = new System.Drawing.Point(295, 147);
            this.deleteLogButton.Name = "deleteLogButton";
            this.deleteLogButton.Size = new System.Drawing.Size(135, 27);
            this.deleteLogButton.TabIndex = 4;
            this.deleteLogButton.Text = "Delete";
            this.deleteLogButton.UseVisualStyleBackColor = true;
            this.deleteLogButton.Click += new System.EventHandler(this.LogBTNDelete_Click);
            // 
            // LogOpenTooltip
            // 
            this.LogOpenTooltip.Tag = "Opens with the system\'s default text editor";
            // 
            // logFilesTreeView
            // 
            this.logFilesTreeView.Location = new System.Drawing.Point(12, 81);
            this.logFilesTreeView.Name = "logFilesTreeView";
            this.logFilesTreeView.Size = new System.Drawing.Size(277, 316);
            this.logFilesTreeView.TabIndex = 5;
            this.logFilesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.LogTVLogfiles_AfterSelect);
            // 
            // deleteAllLogButton
            // 
            this.deleteAllLogButton.Location = new System.Drawing.Point(295, 208);
            this.deleteAllLogButton.Name = "deleteAllLogButton";
            this.deleteAllLogButton.Size = new System.Drawing.Size(135, 27);
            this.deleteAllLogButton.TabIndex = 6;
            this.deleteAllLogButton.Text = "Delete all logs";
            this.deleteAllLogButton.UseVisualStyleBackColor = true;
            this.deleteAllLogButton.Click += new System.EventHandler(this.LogBTNDeleteAll_Click);
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 443);
            this.Controls.Add(this.deleteAllLogButton);
            this.Controls.Add(this.logFilesTreeView);
            this.Controls.Add(this.deleteLogButton);
            this.Controls.Add(this.viewLogButton);
            this.Controls.Add(this.openLogButton);
            this.Controls.Add(this.label1);
            this.Name = "LogForm";
            this.Text = "Logging";
            this.Load += new System.EventHandler(this.LogForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button openLogButton;
        private System.Windows.Forms.Button viewLogButton;
        private System.Windows.Forms.Button deleteLogButton;
        private System.Windows.Forms.ToolTip LogOpenTooltip;
        private System.Windows.Forms.TreeView logFilesTreeView;
        private System.Windows.Forms.Button deleteAllLogButton;
    }
}