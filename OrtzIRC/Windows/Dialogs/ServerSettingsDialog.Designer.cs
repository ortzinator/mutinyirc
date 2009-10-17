namespace OrtzIRC
{
    partial class ServerSettingsDialog
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
            this.ircSettingsTree = new System.Windows.Forms.TreeView();
            this.addNetworkMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNetworkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.serverPortsTextBox = new System.Windows.Forms.TextBox();
            this.serverUriTextBox = new System.Windows.Forms.TextBox();
            this.serverDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.serverGroupBox = new System.Windows.Forms.GroupBox();
            this.networkGroupBox = new System.Windows.Forms.GroupBox();
            this.networkNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.networkContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label6 = new System.Windows.Forms.Label();
            this.autoConnectCheckBox = new System.Windows.Forms.CheckBox();
            this.addNetworkMenuStrip.SuspendLayout();
            this.serverGroupBox.SuspendLayout();
            this.networkGroupBox.SuspendLayout();
            this.networkContextMenuStrip.SuspendLayout();
            this.serverContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ircSettingsTree
            // 
            this.ircSettingsTree.ContextMenuStrip = this.addNetworkMenuStrip;
            this.ircSettingsTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.ircSettingsTree.LabelEdit = true;
            this.ircSettingsTree.Location = new System.Drawing.Point(0, 0);
            this.ircSettingsTree.Name = "ircSettingsTree";
            this.ircSettingsTree.Size = new System.Drawing.Size(147, 249);
            this.ircSettingsTree.TabIndex = 0;
            // 
            // addNetworkMenuStrip
            // 
            this.addNetworkMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNetworkToolStripMenuItem});
            this.addNetworkMenuStrip.Name = "addNetworkMenuStrip";
            this.addNetworkMenuStrip.Size = new System.Drawing.Size(145, 26);
            // 
            // addNetworkToolStripMenuItem
            // 
            this.addNetworkToolStripMenuItem.Name = "addNetworkToolStripMenuItem";
            this.addNetworkToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.addNetworkToolStripMenuItem.Text = "Add Network";
            this.addNetworkToolStripMenuItem.Click += new System.EventHandler(this.addNetworkToolStripMenuItem_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "&Ports:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "&URL:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(51, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "&Description:";
            // 
            // serverPortsTextBox
            // 
            this.serverPortsTextBox.Location = new System.Drawing.Point(125, 93);
            this.serverPortsTextBox.Name = "serverPortsTextBox";
            this.serverPortsTextBox.Size = new System.Drawing.Size(155, 20);
            this.serverPortsTextBox.TabIndex = 2;
            // 
            // serverUriTextBox
            // 
            this.serverUriTextBox.Location = new System.Drawing.Point(125, 66);
            this.serverUriTextBox.Name = "serverUriTextBox";
            this.serverUriTextBox.Size = new System.Drawing.Size(155, 20);
            this.serverUriTextBox.TabIndex = 1;
            // 
            // serverDescriptionTextBox
            // 
            this.serverDescriptionTextBox.Location = new System.Drawing.Point(125, 39);
            this.serverDescriptionTextBox.Name = "serverDescriptionTextBox";
            this.serverDescriptionTextBox.Size = new System.Drawing.Size(155, 20);
            this.serverDescriptionTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(430, 233);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "EXPAND >>";
            this.label1.Visible = false;
            // 
            // serverGroupBox
            // 
            this.serverGroupBox.Controls.Add(this.autoConnectCheckBox);
            this.serverGroupBox.Controls.Add(this.label6);
            this.serverGroupBox.Controls.Add(this.label4);
            this.serverGroupBox.Controls.Add(this.label2);
            this.serverGroupBox.Controls.Add(this.label3);
            this.serverGroupBox.Controls.Add(this.serverDescriptionTextBox);
            this.serverGroupBox.Controls.Add(this.serverUriTextBox);
            this.serverGroupBox.Controls.Add(this.serverPortsTextBox);
            this.serverGroupBox.Location = new System.Drawing.Point(153, 12);
            this.serverGroupBox.Name = "serverGroupBox";
            this.serverGroupBox.Size = new System.Drawing.Size(343, 195);
            this.serverGroupBox.TabIndex = 4;
            this.serverGroupBox.TabStop = false;
            this.serverGroupBox.Text = "Server";
            // 
            // networkGroupBox
            // 
            this.networkGroupBox.Controls.Add(this.networkNameTextBox);
            this.networkGroupBox.Controls.Add(this.label5);
            this.networkGroupBox.Location = new System.Drawing.Point(502, 12);
            this.networkGroupBox.Name = "networkGroupBox";
            this.networkGroupBox.Size = new System.Drawing.Size(343, 195);
            this.networkGroupBox.TabIndex = 5;
            this.networkGroupBox.TabStop = false;
            this.networkGroupBox.Text = "Network";
            // 
            // networkNameTextBox
            // 
            this.networkNameTextBox.Location = new System.Drawing.Point(115, 70);
            this.networkNameTextBox.Name = "networkNameTextBox";
            this.networkNameTextBox.Size = new System.Drawing.Size(133, 20);
            this.networkNameTextBox.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(60, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Name:";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(421, 213);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // networkContextMenuStrip
            // 
            this.networkContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addServerToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.networkContextMenuStrip.Name = "networkContextMenuStrip";
            this.networkContextMenuStrip.Size = new System.Drawing.Size(132, 48);
            // 
            // addServerToolStripMenuItem
            // 
            this.addServerToolStripMenuItem.Name = "addServerToolStripMenuItem";
            this.addServerToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.addServerToolStripMenuItem.Text = "Add Server";
            this.addServerToolStripMenuItem.Click += new System.EventHandler(this.addServerToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // serverContextMenuStrip
            // 
            this.serverContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem1});
            this.serverContextMenuStrip.Name = "serverContextMenuStrip";
            this.serverContextMenuStrip.Size = new System.Drawing.Size(108, 26);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(51, 123);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Autoconnect:";
            // 
            // autoConnectCheckBox
            // 
            this.autoConnectCheckBox.AutoSize = true;
            this.autoConnectCheckBox.Location = new System.Drawing.Point(125, 122);
            this.autoConnectCheckBox.Name = "autoConnectCheckBox";
            this.autoConnectCheckBox.Size = new System.Drawing.Size(15, 14);
            this.autoConnectCheckBox.TabIndex = 7;
            this.autoConnectCheckBox.UseVisualStyleBackColor = true;
            this.autoConnectCheckBox.CheckedChanged += new System.EventHandler(this.autoConnectCheckBox_CheckedChanged);
            // 
            // ServerSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 249);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.networkGroupBox);
            this.Controls.Add(this.serverGroupBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ircSettingsTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ServerSettingsDialog";
            this.Text = "Servers";
            this.addNetworkMenuStrip.ResumeLayout(false);
            this.serverGroupBox.ResumeLayout(false);
            this.serverGroupBox.PerformLayout();
            this.networkGroupBox.ResumeLayout(false);
            this.networkGroupBox.PerformLayout();
            this.networkContextMenuStrip.ResumeLayout(false);
            this.serverContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView ircSettingsTree;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverPortsTextBox;
        private System.Windows.Forms.TextBox serverUriTextBox;
        private System.Windows.Forms.TextBox serverDescriptionTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox serverGroupBox;
        private System.Windows.Forms.GroupBox networkGroupBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox networkNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ContextMenuStrip addNetworkMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addNetworkToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip networkContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip serverContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.CheckBox autoConnectCheckBox;
        private System.Windows.Forms.Label label6;
    }
}