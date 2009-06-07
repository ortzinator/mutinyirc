namespace OrtzIRC
{
    partial class ServerDialog
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
            this.serverTree = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // serverTree
            // 
            this.serverTree.Location = new System.Drawing.Point(12, 12);
            this.serverTree.Name = "serverTree";
            this.serverTree.Size = new System.Drawing.Size(147, 436);
            this.serverTree.TabIndex = 0;
            // 
            // ServerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 460);
            this.Controls.Add(this.serverTree);
            this.Name = "ServerDialog";
            this.Text = "ServerDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView serverTree;




    }
}