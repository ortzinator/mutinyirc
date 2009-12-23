namespace OrtzIRC
{
    partial class GeneralOptionsPage
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.primaryNickTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.secondaryNickTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.backupNickTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(331, 107);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.15385F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.84615F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.primaryNickTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.secondaryNickTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.backupNickTextBox, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(325, 88);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Primary Nick:";
            // 
            // primaryNickTextBox
            // 
            this.primaryNickTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::OrtzIRC.Properties.Settings.Default, "FirstNick", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.primaryNickTextBox.Location = new System.Drawing.Point(114, 3);
            this.primaryNickTextBox.Name = "primaryNickTextBox";
            this.primaryNickTextBox.Size = new System.Drawing.Size(177, 20);
            this.primaryNickTextBox.TabIndex = 1;
            this.primaryNickTextBox.Text = global::OrtzIRC.Properties.Settings.Default.FirstNick;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Secondary Nick:";
            // 
            // secondaryNickTextBox
            // 
            this.secondaryNickTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::OrtzIRC.Properties.Settings.Default, "SecondNick", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.secondaryNickTextBox.Location = new System.Drawing.Point(114, 29);
            this.secondaryNickTextBox.Name = "secondaryNickTextBox";
            this.secondaryNickTextBox.Size = new System.Drawing.Size(177, 20);
            this.secondaryNickTextBox.TabIndex = 3;
            this.secondaryNickTextBox.Text = global::OrtzIRC.Properties.Settings.Default.SecondNick;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Backup Nick:";
            // 
            // backupNickTextBox
            // 
            this.backupNickTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::OrtzIRC.Properties.Settings.Default, "ThirdNick", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.backupNickTextBox.Location = new System.Drawing.Point(114, 55);
            this.backupNickTextBox.Name = "backupNickTextBox";
            this.backupNickTextBox.Size = new System.Drawing.Size(177, 20);
            this.backupNickTextBox.TabIndex = 5;
            this.backupNickTextBox.Text = global::OrtzIRC.Properties.Settings.Default.ThirdNick;
            // 
            // GeneralOptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox1);
            this.Name = "GeneralOptionsPage";
            this.Size = new System.Drawing.Size(331, 107);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox primaryNickTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox secondaryNickTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox backupNickTextBox;
    }
}
