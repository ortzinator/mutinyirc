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
            this.LogBTNOpen = new System.Windows.Forms.Button();
            this.LogBTNView = new System.Windows.Forms.Button();
            this.LogBTNDelete = new System.Windows.Forms.Button();
            this.LogOpenTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.LogTVLogfiles = new System.Windows.Forms.TreeView();
            this.LogBTNDeleteAll = new System.Windows.Forms.Button();
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
            // LogBTNOpen
            // 
            this.LogBTNOpen.Location = new System.Drawing.Point(295, 81);
            this.LogBTNOpen.Name = "LogBTNOpen";
            this.LogBTNOpen.Size = new System.Drawing.Size(135, 27);
            this.LogBTNOpen.TabIndex = 1;
            this.LogBTNOpen.Text = "Open";
            this.LogBTNOpen.UseVisualStyleBackColor = true;
            this.LogBTNOpen.Click += new System.EventHandler(this.LogBTNOpen_Click);
            // 
            // LogBTNView
            // 
            this.LogBTNView.Location = new System.Drawing.Point(295, 114);
            this.LogBTNView.Name = "LogBTNView";
            this.LogBTNView.Size = new System.Drawing.Size(135, 27);
            this.LogBTNView.TabIndex = 2;
            this.LogBTNView.Text = "View";
            this.LogBTNView.UseVisualStyleBackColor = true;
            this.LogBTNView.Click += new System.EventHandler(this.LogBTNView_Click);
            // 
            // LogBTNDelete
            // 
            this.LogBTNDelete.Location = new System.Drawing.Point(295, 147);
            this.LogBTNDelete.Name = "LogBTNDelete";
            this.LogBTNDelete.Size = new System.Drawing.Size(135, 27);
            this.LogBTNDelete.TabIndex = 4;
            this.LogBTNDelete.Text = "Delete";
            this.LogBTNDelete.UseVisualStyleBackColor = true;
            this.LogBTNDelete.Click += new System.EventHandler(this.LogBTNDelete_Click);
            // 
            // LogOpenTooltip
            // 
            this.LogOpenTooltip.Tag = "Opens with the system\'s default text editor";
            // 
            // LogTVLogfiles
            // 
            this.LogTVLogfiles.Location = new System.Drawing.Point(12, 81);
            this.LogTVLogfiles.Name = "LogTVLogfiles";
            this.LogTVLogfiles.Size = new System.Drawing.Size(277, 316);
            this.LogTVLogfiles.TabIndex = 5;
            this.LogTVLogfiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.LogTVLogfiles_AfterSelect);
            // 
            // LogBTNDeleteAll
            // 
            this.LogBTNDeleteAll.Location = new System.Drawing.Point(295, 208);
            this.LogBTNDeleteAll.Name = "LogBTNDeleteAll";
            this.LogBTNDeleteAll.Size = new System.Drawing.Size(135, 27);
            this.LogBTNDeleteAll.TabIndex = 6;
            this.LogBTNDeleteAll.Text = "Delete all logs";
            this.LogBTNDeleteAll.UseVisualStyleBackColor = true;
            this.LogBTNDeleteAll.Click += new System.EventHandler(this.LogBTNDeleteAll_Click);
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 443);
            this.Controls.Add(this.LogBTNDeleteAll);
            this.Controls.Add(this.LogTVLogfiles);
            this.Controls.Add(this.LogBTNDelete);
            this.Controls.Add(this.LogBTNView);
            this.Controls.Add(this.LogBTNOpen);
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
        private System.Windows.Forms.Button LogBTNOpen;
        private System.Windows.Forms.Button LogBTNView;
        private System.Windows.Forms.Button LogBTNDelete;
        private System.Windows.Forms.ToolTip LogOpenTooltip;
        private System.Windows.Forms.TreeView LogTVLogfiles;
        private System.Windows.Forms.Button LogBTNDeleteAll;
    }
}