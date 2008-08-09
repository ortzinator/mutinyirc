using OrtzIRC.Controls;
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
            this.serverLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.serverOutputBox = new OrtzIRC.Controls.ChannelText();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.serverLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // serverLayoutPanel
            // 
            this.serverLayoutPanel.ColumnCount = 1;
            this.serverLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.serverLayoutPanel.Controls.Add(this.serverOutputBox, 0, 0);
            this.serverLayoutPanel.Controls.Add(this.commandTextBox, 0, 1);
            this.serverLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.serverLayoutPanel.Name = "serverLayoutPanel";
            this.serverLayoutPanel.RowCount = 2;
            this.serverLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.serverLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.serverLayoutPanel.Size = new System.Drawing.Size(582, 429);
            this.serverLayoutPanel.TabIndex = 1;
            // 
            // serverOutputBox
            // 
            this.serverOutputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverOutputBox.Location = new System.Drawing.Point(3, 3);
            this.serverOutputBox.Name = "serverOutputBox";
            this.serverOutputBox.ReadOnly = true;
            this.serverOutputBox.Size = new System.Drawing.Size(576, 396);
            this.serverOutputBox.TabIndex = 0;
            this.serverOutputBox.Text = "";
            // 
            // commandTextBox
            // 
            this.commandTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandTextBox.Location = new System.Drawing.Point(3, 405);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(576, 20);
            this.commandTextBox.TabIndex = 1;
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 429);
            this.Controls.Add(this.serverLayoutPanel);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.serverLayoutPanel.ResumeLayout(false);
            this.serverLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel serverLayoutPanel;
        private ChannelText serverOutputBox;
        private System.Windows.Forms.TextBox commandTextBox;

    }
}