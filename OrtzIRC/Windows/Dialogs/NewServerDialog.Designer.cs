namespace OrtzIRC
{
    partial class NewServerDialog
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
            this.descTextBox = new System.Windows.Forms.TextBox();
            this.uriTextBox = new System.Windows.Forms.TextBox();
            this.descLabel = new System.Windows.Forms.Label();
            this.serverLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // descTextBox
            // 
            this.descTextBox.Location = new System.Drawing.Point(89, 12);
            this.descTextBox.Name = "descTextBox";
            this.descTextBox.Size = new System.Drawing.Size(146, 20);
            this.descTextBox.TabIndex = 1;
            // 
            // uriTextBox
            // 
            this.uriTextBox.Location = new System.Drawing.Point(89, 39);
            this.uriTextBox.Name = "uriTextBox";
            this.uriTextBox.Size = new System.Drawing.Size(146, 20);
            this.uriTextBox.TabIndex = 3;
            // 
            // descLabel
            // 
            this.descLabel.AutoSize = true;
            this.descLabel.Location = new System.Drawing.Point(20, 15);
            this.descLabel.Name = "descLabel";
            this.descLabel.Size = new System.Drawing.Size(63, 13);
            this.descLabel.TabIndex = 0;
            this.descLabel.Text = "Description:";
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(21, 42);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(62, 13);
            this.serverLabel.TabIndex = 2;
            this.serverLabel.Text = "IRC Server:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(89, 66);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(146, 20);
            this.portTextBox.TabIndex = 5;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(54, 69);
            this.portLabel.Name = "portLabel";
            this.portLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 4;
            this.portLabel.Text = "Ports:";
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(49, 110);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 6;
            this.addButton.Text = "Add Server";
            this.addButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(130, 110);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // NewServerDialog
            // 
            this.AcceptButton = this.addButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(254, 156);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.descTextBox);
            this.Controls.Add(this.serverLabel);
            this.Controls.Add(this.uriTextBox);
            this.Controls.Add(this.descLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewServerDialog";
            this.Text = "Add New Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Label descLabel;
        private System.Windows.Forms.TextBox uriTextBox;
        private System.Windows.Forms.TextBox descTextBox;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button cancelButton;
    }
}