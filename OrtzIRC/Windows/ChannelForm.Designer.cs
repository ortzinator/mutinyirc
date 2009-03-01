namespace OrtzIRC
{
    partial class ChannelForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.channelOutputBox = new OrtzIRC.IrcTextBox();
            this.nickListBox = new OrtzIRC.NickListBox();
            this.commandTextBox = new OrtzIRC.CommandTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.channelOutputBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.nickListBox);
            this.splitContainer1.Size = new System.Drawing.Size(684, 542);
            this.splitContainer1.SplitterDistance = 521;
            this.splitContainer1.TabIndex = 4;
            // 
            // channelOutputBox
            // 
            this.channelOutputBox.BackColor = System.Drawing.SystemColors.Window;
            this.channelOutputBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.channelOutputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.channelOutputBox.Location = new System.Drawing.Point(0, 0);
            this.channelOutputBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.channelOutputBox.Name = "channelOutputBox";
            this.channelOutputBox.ReadOnly = true;
            this.channelOutputBox.Size = new System.Drawing.Size(521, 542);
            this.channelOutputBox.TabIndex = 1;
            this.channelOutputBox.Text = "";
            // 
            // nickListBox
            // 
            this.nickListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nickListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.nickListBox.FormattingEnabled = true;
            this.nickListBox.Location = new System.Drawing.Point(0, 0);
            this.nickListBox.Name = "nickListBox";
            this.nickListBox.OpColor = System.Drawing.Color.Black;
            this.nickListBox.RegularUserColor = System.Drawing.Color.Black;
            this.nickListBox.Size = new System.Drawing.Size(159, 537);
            this.nickListBox.Sorted = true;
            this.nickListBox.TabIndex = 2;
            this.nickListBox.UserList = null;
            this.nickListBox.VoiceColor = System.Drawing.Color.Black;
            // 
            // commandTextBox
            // 
            this.commandTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commandTextBox.Location = new System.Drawing.Point(0, 542);
            this.commandTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(684, 22);
            this.commandTextBox.TabIndex = 0;
            // 
            // ChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 564);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.commandTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ChannelForm";
            this.Text = "ChannelForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OrtzIRC.CommandTextBox commandTextBox;
        private OrtzIRC.IrcTextBox channelOutputBox;
        private OrtzIRC.NickListBox nickListBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}