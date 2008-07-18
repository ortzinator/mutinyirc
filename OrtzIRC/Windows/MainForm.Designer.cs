namespace OrtzIRC
{
    partial class MainForm
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
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileRootMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serversMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowRootMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpRootMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(390, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "mainToolbar";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileRootMenuItem,
            this.windowRootMenuItem,
            this.helpRootMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.MdiWindowListItem = this.windowRootMenuItem;
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(390, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "mainMenu";
            // 
            // fileRootMenuItem
            // 
            this.fileRootMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serversMenuItem,
            this.exitMenuItem});
            this.fileRootMenuItem.Name = "fileRootMenuItem";
            this.fileRootMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileRootMenuItem.Text = "&File";
            // 
            // serversMenuItem
            // 
            this.serversMenuItem.Name = "serversMenuItem";
            this.serversMenuItem.Size = new System.Drawing.Size(152, 22);
            this.serversMenuItem.Text = "Servers...";
            this.serversMenuItem.Click += new System.EventHandler(this.newServerMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // windowRootMenuItem
            // 
            this.windowRootMenuItem.Name = "windowRootMenuItem";
            this.windowRootMenuItem.Size = new System.Drawing.Size(57, 20);
            this.windowRootMenuItem.Text = "&Window";
            // 
            // helpRootMenuItem
            // 
            this.helpRootMenuItem.Name = "helpRootMenuItem";
            this.helpRootMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.helpRootMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpRootMenuItem.Text = "&Help";
            this.helpRootMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 319);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "OrtzIRC";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileRootMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpRootMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowRootMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serversMenuItem;
    }
}

