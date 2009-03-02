namespace OrtzIRC
{
    partial class LogForm
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
            this.noteEnableLogLabel = new System.Windows.Forms.Label();
            this.openLogButton = new System.Windows.Forms.Button();
            this.viewLogButton = new System.Windows.Forms.Button();
            this.deleteLogButton = new System.Windows.Forms.Button();
            this.LogOpenTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.logFilesTreeView = new System.Windows.Forms.TreeView();
            this.deleteAllLogButton = new System.Windows.Forms.Button();
            this.timestampFormatLabel = new System.Windows.Forms.Label();
            this.timestampFormatTextbox = new System.Windows.Forms.TextBox();
            this.addTimestampCheckbox = new System.Windows.Forms.CheckBox();
            this.activateLoggerCheckBox = new System.Windows.Forms.CheckBox();
            this.settingsChangedLabel = new System.Windows.Forms.Label();
            this.timestampExampleLabel = new System.Windows.Forms.Label();
            this.timestampExampleTextLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // noteEnableLogLabel
            // 
            this.noteEnableLogLabel.AutoSize = true;
            this.noteEnableLogLabel.Location = new System.Drawing.Point(12, 466);
            this.noteEnableLogLabel.Name = "noteEnableLogLabel";
            this.noteEnableLogLabel.Size = new System.Drawing.Size(321, 13);
            this.noteEnableLogLabel.TabIndex = 0;
            this.noteEnableLogLabel.Text = "Note: Logging can be enabled or disabled within the options dialog";
            // 
            // openLogButton
            // 
            this.openLogButton.Location = new System.Drawing.Point(295, 128);
            this.openLogButton.Name = "openLogButton";
            this.openLogButton.Size = new System.Drawing.Size(135, 27);
            this.openLogButton.TabIndex = 1;
            this.openLogButton.Text = "&Open";
            this.openLogButton.UseVisualStyleBackColor = true;
            this.openLogButton.Click += new System.EventHandler(this.LogBTNOpen_Click);
            // 
            // viewLogButton
            // 
            this.viewLogButton.Location = new System.Drawing.Point(295, 161);
            this.viewLogButton.Name = "viewLogButton";
            this.viewLogButton.Size = new System.Drawing.Size(135, 27);
            this.viewLogButton.TabIndex = 2;
            this.viewLogButton.Text = "&View";
            this.viewLogButton.UseVisualStyleBackColor = true;
            this.viewLogButton.Click += new System.EventHandler(this.LogBTNView_Click);
            // 
            // deleteLogButton
            // 
            this.deleteLogButton.Location = new System.Drawing.Point(295, 194);
            this.deleteLogButton.Name = "deleteLogButton";
            this.deleteLogButton.Size = new System.Drawing.Size(135, 27);
            this.deleteLogButton.TabIndex = 3;
            this.deleteLogButton.Text = "&Delete";
            this.deleteLogButton.UseVisualStyleBackColor = true;
            this.deleteLogButton.Click += new System.EventHandler(this.LogBTNDelete_Click);
            // 
            // LogOpenTooltip
            // 
            this.LogOpenTooltip.Tag = "Opens with the system\'s default text editor";
            // 
            // logFilesTreeView
            // 
            this.logFilesTreeView.Location = new System.Drawing.Point(12, 128);
            this.logFilesTreeView.Name = "logFilesTreeView";
            this.logFilesTreeView.Size = new System.Drawing.Size(277, 316);
            this.logFilesTreeView.TabIndex = 0;
            this.logFilesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.LogTVLogfiles_AfterSelect);
            // 
            // deleteAllLogButton
            // 
            this.deleteAllLogButton.Location = new System.Drawing.Point(295, 255);
            this.deleteAllLogButton.Name = "deleteAllLogButton";
            this.deleteAllLogButton.Size = new System.Drawing.Size(135, 27);
            this.deleteAllLogButton.TabIndex = 4;
            this.deleteAllLogButton.Text = "Delete &all logs";
            this.deleteAllLogButton.UseVisualStyleBackColor = true;
            this.deleteAllLogButton.Click += new System.EventHandler(this.LogBTNDeleteAll_Click);
            // 
            // timestampFormatLabel
            // 
            this.timestampFormatLabel.AutoSize = true;
            this.timestampFormatLabel.Location = new System.Drawing.Point(29, 55);
            this.timestampFormatLabel.Name = "timestampFormatLabel";
            this.timestampFormatLabel.Size = new System.Drawing.Size(96, 13);
            this.timestampFormatLabel.TabIndex = 8;
            this.timestampFormatLabel.Text = "Timestamp Format:";
            // 
            // timestampFormatTextbox
            // 
            this.timestampFormatTextbox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::OrtzIRC.Properties.Settings.Default, "LoggerTimestampFormat", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.timestampFormatTextbox.Location = new System.Drawing.Point(142, 52);
            this.timestampFormatTextbox.Name = "timestampFormatTextbox";
            this.timestampFormatTextbox.Size = new System.Drawing.Size(263, 20);
            this.timestampFormatTextbox.TabIndex = 9;
            this.timestampFormatTextbox.Text = global::OrtzIRC.Properties.Settings.Default.LoggerTimestampFormat;
            this.timestampFormatTextbox.TextChanged += new System.EventHandler(this.timestampFormatTextbox_TextChanged);
            // 
            // addTimestampCheckbox
            // 
            this.addTimestampCheckbox.AutoSize = true;
            this.addTimestampCheckbox.Checked = global::OrtzIRC.Properties.Settings.Default.LoggerTimestampsActivated;
            this.addTimestampCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.addTimestampCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::OrtzIRC.Properties.Settings.Default, "LoggerTimestampsActivated", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.addTimestampCheckbox.Location = new System.Drawing.Point(12, 35);
            this.addTimestampCheckbox.Name = "addTimestampCheckbox";
            this.addTimestampCheckbox.Size = new System.Drawing.Size(95, 17);
            this.addTimestampCheckbox.TabIndex = 10;
            this.addTimestampCheckbox.Text = "Add timestamp";
            this.addTimestampCheckbox.UseVisualStyleBackColor = true;
            this.addTimestampCheckbox.CheckedChanged += new System.EventHandler(this.addTimestampCheckbox_CheckedChanged);
            // 
            // activateLoggerCheckBox
            // 
            this.activateLoggerCheckBox.AutoSize = true;
            this.activateLoggerCheckBox.Checked = global::OrtzIRC.Properties.Settings.Default.LoggerActivated;
            this.activateLoggerCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::OrtzIRC.Properties.Settings.Default, "LoggerActivated", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.activateLoggerCheckBox.Location = new System.Drawing.Point(12, 12);
            this.activateLoggerCheckBox.Name = "activateLoggerCheckBox";
            this.activateLoggerCheckBox.Size = new System.Drawing.Size(410, 17);
            this.activateLoggerCheckBox.TabIndex = 8;
            this.activateLoggerCheckBox.Text = "Activate Logger (SETTINGS HERE WILL BE MOVED TO AN OPTIONS PANEL)";
            this.activateLoggerCheckBox.UseVisualStyleBackColor = true;
            this.activateLoggerCheckBox.CheckedChanged += new System.EventHandler(this.activateLoggerCheckBox_CheckedChanged);
            // 
            // settingsChangedLabel
            // 
            this.settingsChangedLabel.AutoSize = true;
            this.settingsChangedLabel.Location = new System.Drawing.Point(12, 103);
            this.settingsChangedLabel.Name = "settingsChangedLabel";
            this.settingsChangedLabel.Size = new System.Drawing.Size(239, 13);
            this.settingsChangedLabel.TabIndex = 10;
            this.settingsChangedLabel.Text = "Settings will be saved when you close this dialog.";
            this.settingsChangedLabel.Visible = false;
            // 
            // timestampExampleLabel
            // 
            this.timestampExampleLabel.AutoSize = true;
            this.timestampExampleLabel.Location = new System.Drawing.Point(29, 78);
            this.timestampExampleLabel.Name = "timestampExampleLabel";
            this.timestampExampleLabel.Size = new System.Drawing.Size(104, 13);
            this.timestampExampleLabel.TabIndex = 11;
            this.timestampExampleLabel.Text = "Timestamp Example:";
            // 
            // timestampExampleTextLabel
            // 
            this.timestampExampleTextLabel.AutoSize = true;
            this.timestampExampleTextLabel.Location = new System.Drawing.Point(139, 78);
            this.timestampExampleTextLabel.Name = "timestampExampleTextLabel";
            this.timestampExampleTextLabel.Size = new System.Drawing.Size(0, 13);
            this.timestampExampleTextLabel.TabIndex = 12;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 491);
            this.Controls.Add(this.timestampExampleTextLabel);
            this.Controls.Add(this.timestampExampleLabel);
            this.Controls.Add(this.settingsChangedLabel);
            this.Controls.Add(this.timestampFormatTextbox);
            this.Controls.Add(this.timestampFormatLabel);
            this.Controls.Add(this.addTimestampCheckbox);
            this.Controls.Add(this.activateLoggerCheckBox);
            this.Controls.Add(this.deleteAllLogButton);
            this.Controls.Add(this.logFilesTreeView);
            this.Controls.Add(this.deleteLogButton);
            this.Controls.Add(this.viewLogButton);
            this.Controls.Add(this.openLogButton);
            this.Controls.Add(this.noteEnableLogLabel);
            this.Name = "LogForm";
            this.Text = "Logging";
            this.Load += new System.EventHandler(this.LogForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label noteEnableLogLabel;
        private System.Windows.Forms.Button openLogButton;
        private System.Windows.Forms.Button viewLogButton;
        private System.Windows.Forms.Button deleteLogButton;
        private System.Windows.Forms.ToolTip LogOpenTooltip;
        private System.Windows.Forms.TreeView logFilesTreeView;
        private System.Windows.Forms.Button deleteAllLogButton;
        private System.Windows.Forms.CheckBox activateLoggerCheckBox;
        private System.Windows.Forms.CheckBox addTimestampCheckbox;
        private System.Windows.Forms.Label timestampFormatLabel;
        private System.Windows.Forms.TextBox timestampFormatTextbox;
        private System.Windows.Forms.Label settingsChangedLabel;
        private System.Windows.Forms.Label timestampExampleLabel;
        private System.Windows.Forms.Label timestampExampleTextLabel;
    }
}