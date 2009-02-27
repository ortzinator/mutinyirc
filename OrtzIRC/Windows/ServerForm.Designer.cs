namespace OrtzIRC
{
    partial class ServerForm
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
            this.serverOutputBox = new OrtzIRC.IrcTextBox();
            this.commandTextBox = new OrtzIRC.CommandTextBox();
            this.SuspendLayout();
            // 
            // serverOutputBox
            // 
            this.serverOutputBox.BackColor = System.Drawing.SystemColors.Window;
            this.serverOutputBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.serverOutputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverOutputBox.Location = new System.Drawing.Point(0, 0);
            this.serverOutputBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverOutputBox.Name = "serverOutputBox";
            this.serverOutputBox.ReadOnly = true;
            this.serverOutputBox.Size = new System.Drawing.Size(684, 541);
            this.serverOutputBox.TabIndex = 0;
            this.serverOutputBox.Text = "";
            // 
            // commandTextBox
            // 
            this.commandTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commandTextBox.Location = new System.Drawing.Point(0, 541);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(684, 23);
            this.commandTextBox.TabIndex = 1;
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 564);
            this.Controls.Add(this.serverOutputBox);
            this.Controls.Add(this.commandTextBox);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private IrcTextBox serverOutputBox;
        private CommandTextBox commandTextBox;

    }
}