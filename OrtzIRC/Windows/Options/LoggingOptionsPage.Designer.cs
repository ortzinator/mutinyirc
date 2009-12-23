namespace OrtzIRC
{
    partial class LoggingOptionsPage
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
            this.timestampExampleTextLabel = new System.Windows.Forms.Label();
            this.timestampExampleLabel = new System.Windows.Forms.Label();
            this.timestampFormatTextbox = new System.Windows.Forms.TextBox();
            this.timestampFormatLabel = new System.Windows.Forms.Label();
            this.addTimestampCheckbox = new System.Windows.Forms.CheckBox();
            this.activateLoggerCheckBox = new System.Windows.Forms.CheckBox();
            this.deleteAllLogButton = new System.Windows.Forms.Button();
            this.logFilesTreeView = new System.Windows.Forms.TreeView();
            this.deleteLogButton = new System.Windows.Forms.Button();
            this.viewLogButton = new System.Windows.Forms.Button();
            this.LogOpenTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.openLogButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timestampExampleTextLabel
            // 
            this.timestampExampleTextLabel.AutoSize = true;
            this.timestampExampleTextLabel.Location = new System.Drawing.Point(130, 69);
            this.timestampExampleTextLabel.Name = "timestampExampleTextLabel";
            this.timestampExampleTextLabel.Size = new System.Drawing.Size(0, 13);
            this.timestampExampleTextLabel.TabIndex = 25;
            // 
            // timestampExampleLabel
            // 
            this.timestampExampleLabel.AutoSize = true;
            this.timestampExampleLabel.Location = new System.Drawing.Point(20, 69);
            this.timestampExampleLabel.Name = "timestampExampleLabel";
            this.timestampExampleLabel.Size = new System.Drawing.Size(104, 13);
            this.timestampExampleLabel.TabIndex = 24;
            this.timestampExampleLabel.Text = "Timestamp Example:";
            // 
            // timestampFormatTextbox
            // 
            this.timestampFormatTextbox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::OrtzIRC.Properties.Settings.Default, "LoggerTimestampFormat", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.timestampFormatTextbox.Location = new System.Drawing.Point(133, 43);
            this.timestampFormatTextbox.Name = "timestampFormatTextbox";
            this.timestampFormatTextbox.Size = new System.Drawing.Size(263, 20);
            this.timestampFormatTextbox.TabIndex = 21;
            this.timestampFormatTextbox.Text = global::OrtzIRC.Properties.Settings.Default.LoggerTimestampFormat;
            this.timestampFormatTextbox.TextChanged += new System.EventHandler(this.timestampFormatTextbox_TextChanged);
            // 
            // timestampFormatLabel
            // 
            this.timestampFormatLabel.AutoSize = true;
            this.timestampFormatLabel.Location = new System.Drawing.Point(20, 46);
            this.timestampFormatLabel.Name = "timestampFormatLabel";
            this.timestampFormatLabel.Size = new System.Drawing.Size(96, 13);
            this.timestampFormatLabel.TabIndex = 19;
            this.timestampFormatLabel.Text = "Timestamp Format:";
            // 
            // addTimestampCheckbox
            // 
            this.addTimestampCheckbox.AutoSize = true;
            this.addTimestampCheckbox.Checked = global::OrtzIRC.Properties.Settings.Default.LoggerTimestampsActivated;
            this.addTimestampCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.addTimestampCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::OrtzIRC.Properties.Settings.Default, "LoggerTimestampsActivated", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.addTimestampCheckbox.Location = new System.Drawing.Point(3, 26);
            this.addTimestampCheckbox.Name = "addTimestampCheckbox";
            this.addTimestampCheckbox.Size = new System.Drawing.Size(95, 17);
            this.addTimestampCheckbox.TabIndex = 23;
            this.addTimestampCheckbox.Text = "Add timestamp";
            this.addTimestampCheckbox.UseVisualStyleBackColor = true;
            // 
            // activateLoggerCheckBox
            // 
            this.activateLoggerCheckBox.AutoSize = true;
            this.activateLoggerCheckBox.Checked = global::OrtzIRC.Properties.Settings.Default.LoggerActivated;
            this.activateLoggerCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::OrtzIRC.Properties.Settings.Default, "LoggerActivated", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.activateLoggerCheckBox.Location = new System.Drawing.Point(3, 3);
            this.activateLoggerCheckBox.Name = "activateLoggerCheckBox";
            this.activateLoggerCheckBox.Size = new System.Drawing.Size(101, 17);
            this.activateLoggerCheckBox.TabIndex = 20;
            this.activateLoggerCheckBox.Text = "Activate Logger";
            this.activateLoggerCheckBox.UseVisualStyleBackColor = true;
            // 
            // deleteAllLogButton
            // 
            this.deleteAllLogButton.Location = new System.Drawing.Point(3, 130);
            this.deleteAllLogButton.Name = "deleteAllLogButton";
            this.deleteAllLogButton.Size = new System.Drawing.Size(110, 27);
            this.deleteAllLogButton.TabIndex = 18;
            this.deleteAllLogButton.Text = "Delete &all logs";
            this.deleteAllLogButton.UseVisualStyleBackColor = true;
            this.deleteAllLogButton.Click += new System.EventHandler(this.deleteAllLogButton_Click);
            // 
            // logFilesTreeView
            // 
            this.logFilesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logFilesTreeView.Location = new System.Drawing.Point(8, 8);
            this.logFilesTreeView.Name = "logFilesTreeView";
            this.logFilesTreeView.Size = new System.Drawing.Size(314, 168);
            this.logFilesTreeView.TabIndex = 13;
            this.logFilesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.logFilesTreeView_AfterSelect);
            // 
            // deleteLogButton
            // 
            this.deleteLogButton.Location = new System.Drawing.Point(3, 69);
            this.deleteLogButton.Name = "deleteLogButton";
            this.deleteLogButton.Size = new System.Drawing.Size(110, 27);
            this.deleteLogButton.TabIndex = 17;
            this.deleteLogButton.Text = "&Delete";
            this.deleteLogButton.UseVisualStyleBackColor = true;
            this.deleteLogButton.Click += new System.EventHandler(this.deleteLogButton_Click);
            // 
            // viewLogButton
            // 
            this.viewLogButton.Location = new System.Drawing.Point(3, 36);
            this.viewLogButton.Name = "viewLogButton";
            this.viewLogButton.Size = new System.Drawing.Size(110, 27);
            this.viewLogButton.TabIndex = 16;
            this.viewLogButton.Text = "&View";
            this.viewLogButton.UseVisualStyleBackColor = true;
            this.viewLogButton.Click += new System.EventHandler(this.viewLogButton_Click);
            // 
            // LogOpenTooltip
            // 
            this.LogOpenTooltip.Tag = "Opens with the system\'s default text editor";
            // 
            // openLogButton
            // 
            this.openLogButton.Location = new System.Drawing.Point(3, 3);
            this.openLogButton.Name = "openLogButton";
            this.openLogButton.Size = new System.Drawing.Size(110, 27);
            this.openLogButton.TabIndex = 15;
            this.openLogButton.Text = "&Open";
            this.openLogButton.UseVisualStyleBackColor = true;
            this.openLogButton.Click += new System.EventHandler(this.openLogButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel3);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(453, 288);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logging";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.logFilesTreeView);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 101);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(8);
            this.panel3.Size = new System.Drawing.Size(330, 184);
            this.panel3.TabIndex = 28;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.openLogButton);
            this.panel2.Controls.Add(this.deleteAllLogButton);
            this.panel2.Controls.Add(this.deleteLogButton);
            this.panel2.Controls.Add(this.viewLogButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(333, 101);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(117, 184);
            this.panel2.TabIndex = 27;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.activateLoggerCheckBox);
            this.panel1.Controls.Add(this.addTimestampCheckbox);
            this.panel1.Controls.Add(this.timestampExampleTextLabel);
            this.panel1.Controls.Add(this.timestampFormatLabel);
            this.panel1.Controls.Add(this.timestampFormatTextbox);
            this.panel1.Controls.Add(this.timestampExampleLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(447, 85);
            this.panel1.TabIndex = 26;
            // 
            // LoggingOptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.groupBox1);
            this.Name = "LoggingOptionsPage";
            this.Size = new System.Drawing.Size(453, 288);
            this.groupBox1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label timestampExampleTextLabel;
        private System.Windows.Forms.Label timestampExampleLabel;
        private System.Windows.Forms.TextBox timestampFormatTextbox;
        private System.Windows.Forms.Label timestampFormatLabel;
        private System.Windows.Forms.CheckBox addTimestampCheckbox;
        private System.Windows.Forms.CheckBox activateLoggerCheckBox;
        private System.Windows.Forms.Button deleteAllLogButton;
        private System.Windows.Forms.TreeView logFilesTreeView;
        private System.Windows.Forms.Button deleteLogButton;
        private System.Windows.Forms.Button viewLogButton;
        private System.Windows.Forms.ToolTip LogOpenTooltip;
        private System.Windows.Forms.Button openLogButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}
