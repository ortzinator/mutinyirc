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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.commandTextBox = new OrtzIRC.Controls.CommandTextBox();
            this.channelOutputBox = new OrtzIRC.Controls.ChannelText();
            this.nickListBox = new OrtzIRC.Controls.NickListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.tableLayoutPanel1.Controls.Add(this.commandTextBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.channelOutputBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.nickListBox, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(829, 523);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // commandTextBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.commandTextBox, 2);
            this.commandTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandTextBox.Location = new System.Drawing.Point(3, 499);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(823, 20);
            this.commandTextBox.TabIndex = 0;
            // 
            // channelOutputBox
            // 
            this.channelOutputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.channelOutputBox.Location = new System.Drawing.Point(3, 3);
            this.channelOutputBox.Name = "channelOutputBox";
            this.channelOutputBox.Size = new System.Drawing.Size(691, 490);
            this.channelOutputBox.TabIndex = 1;
            this.channelOutputBox.Text = "";
            // 
            // nickListBox
            // 
            this.nickListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nickListBox.FormattingEnabled = true;
            this.nickListBox.Location = new System.Drawing.Point(700, 3);
            this.nickListBox.Name = "nickListBox";
            this.nickListBox.Size = new System.Drawing.Size(126, 485);
            this.nickListBox.Sorted = true;
            this.nickListBox.TabIndex = 2;
            // 
            // ChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 523);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ChannelForm";
            this.Text = "ChannelForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private OrtzIRC.Controls.CommandTextBox commandTextBox;
        private OrtzIRC.Controls.ChannelText channelOutputBox;
        private OrtzIRC.Controls.NickListBox nickListBox;
    }
}