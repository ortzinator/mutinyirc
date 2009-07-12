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
            this.settingsTree = new System.Windows.Forms.TreeView();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.serverPortsTextBox = new System.Windows.Forms.TextBox();
            this.serverUriTextBox = new System.Windows.Forms.TextBox();
            this.serverDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.serverGroupBox = new System.Windows.Forms.GroupBox();
            this.networkGroupBox = new System.Windows.Forms.GroupBox();
            this.okButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.networkNameTextBox = new System.Windows.Forms.TextBox();
            this.serverGroupBox.SuspendLayout();
            this.networkGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsTree
            // 
            this.settingsTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.settingsTree.Location = new System.Drawing.Point(0, 0);
            this.settingsTree.Name = "settingsTree";
            this.settingsTree.Size = new System.Drawing.Size(147, 250);
            this.settingsTree.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Ports:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "URI:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(51, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Description:";
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
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(421, 213);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
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
            // networkNameTextBox
            // 
            this.networkNameTextBox.Location = new System.Drawing.Point(115, 70);
            this.networkNameTextBox.Name = "networkNameTextBox";
            this.networkNameTextBox.Size = new System.Drawing.Size(133, 20);
            this.networkNameTextBox.TabIndex = 1;
            // 
            // ServerSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 250);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.networkGroupBox);
            this.Controls.Add(this.serverGroupBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.settingsTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ServerSettingsDialog";
            this.Text = "Servers";
            this.serverGroupBox.ResumeLayout(false);
            this.serverGroupBox.PerformLayout();
            this.networkGroupBox.ResumeLayout(false);
            this.networkGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView settingsTree;
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




    }
}