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
            System.Data.SQLite.SQLiteParameter sqLiteParameter1 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter2 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter3 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter4 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter5 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter6 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter7 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter8 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter9 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter10 = new System.Data.SQLite.SQLiteParameter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSettingsDialog));
            System.Data.SQLite.SQLiteParameter sqLiteParameter11 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter12 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter13 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter14 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter15 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter16 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter17 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter18 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter19 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter20 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter21 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter22 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter23 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter24 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter25 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter26 = new System.Data.SQLite.SQLiteParameter();
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
            this.networkNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.sqliteSelectCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqLiteConnection1 = new System.Data.SQLite.SQLiteConnection();
            this.sqliteInsertCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteUpdateCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteDeleteCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.networkDataAdapter = new System.Data.SQLite.SQLiteDataAdapter();
            this.ircSettingsDataSet1 = new OrtzIRC.IrcSettingsDataSet();
            this.sqliteSelectCommand2 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteInsertCommand2 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteUpdateCommand2 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteDeleteCommand2 = new System.Data.SQLite.SQLiteCommand();
            this.serverDataAdapter = new System.Data.SQLite.SQLiteDataAdapter();
            this.serverGroupBox.SuspendLayout();
            this.networkGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ircSettingsDataSet1)).BeginInit();
            this.SuspendLayout();
            // 
            // settingsTree
            // 
            this.settingsTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.settingsTree.Location = new System.Drawing.Point(0, 0);
            this.settingsTree.Name = "settingsTree";
            this.settingsTree.Size = new System.Drawing.Size(147, 249);
            this.settingsTree.TabIndex = 0;
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
            // sqliteSelectCommand1
            // 
            this.sqliteSelectCommand1.CommandText = "SELECT        Networks.*\r\nFROM            Networks";
            this.sqliteSelectCommand1.Connection = this.sqLiteConnection1;
            // 
            // sqLiteConnection1
            // 
            this.sqLiteConnection1.ConnectionString = "data source=C:\\Users\\Brian\\Documents\\SVN\\OrtzIRC\\OrtzIRC\\bin\\x86\\Debug\\ircsetting" +
                "s.db";
            this.sqLiteConnection1.DefaultTimeout = 30;
            // 
            // sqliteInsertCommand1
            // 
            this.sqliteInsertCommand1.CommandText = "INSERT INTO [Networks] ([Name]) VALUES (@Name)";
            this.sqliteInsertCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter1.ParameterName = "@Name";
            sqLiteParameter1.SourceColumn = "Name";
            this.sqliteInsertCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter1});
            // 
            // sqliteUpdateCommand1
            // 
            this.sqliteUpdateCommand1.CommandText = "UPDATE [Networks] SET [Name] = @Name WHERE (([Id] = @Original_Id) AND ([Name] = @" +
                "Original_Name))";
            this.sqliteUpdateCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter2.ParameterName = "@Name";
            sqLiteParameter2.SourceColumn = "Name";
            sqLiteParameter3.ParameterName = "@Original_Id";
            sqLiteParameter3.SourceColumn = "Id";
            sqLiteParameter3.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter4.ParameterName = "@Original_Name";
            sqLiteParameter4.SourceColumn = "Name";
            sqLiteParameter4.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteUpdateCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter2,
            sqLiteParameter3,
            sqLiteParameter4});
            // 
            // sqliteDeleteCommand1
            // 
            this.sqliteDeleteCommand1.CommandText = "DELETE FROM [Networks] WHERE (([Id] = @Original_Id) AND ([Name] = @Original_Name)" +
                ")";
            this.sqliteDeleteCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter5.ParameterName = "@Original_Id";
            sqLiteParameter5.SourceColumn = "Id";
            sqLiteParameter5.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter6.ParameterName = "@Original_Name";
            sqLiteParameter6.SourceColumn = "Name";
            sqLiteParameter6.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteDeleteCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter5,
            sqLiteParameter6});
            // 
            // networkDataAdapter
            // 
            this.networkDataAdapter.DeleteCommand = this.sqliteDeleteCommand1;
            this.networkDataAdapter.InsertCommand = this.sqliteInsertCommand1;
            this.networkDataAdapter.SelectCommand = this.sqliteSelectCommand1;
            this.networkDataAdapter.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "Networks", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("Id", "Id"),
                        new System.Data.Common.DataColumnMapping("Name", "Name")})});
            this.networkDataAdapter.UpdateCommand = this.sqliteUpdateCommand1;
            // 
            // ircSettingsDataSet1
            // 
            this.ircSettingsDataSet1.DataSetName = "IrcSettingsDataSet";
            this.ircSettingsDataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // sqliteSelectCommand2
            // 
            this.sqliteSelectCommand2.CommandText = "SELECT        Servers.*\r\nFROM            Servers";
            this.sqliteSelectCommand2.Connection = this.sqLiteConnection1;
            // 
            // sqliteInsertCommand2
            // 
            this.sqliteInsertCommand2.CommandText = "INSERT INTO [Servers] ([Description], [Url], [Ports], [NetworkId]) VALUES (@Descr" +
                "iption, @Url, @Ports, @NetworkId)";
            this.sqliteInsertCommand2.Connection = this.sqLiteConnection1;
            sqLiteParameter7.ParameterName = "@Description";
            sqLiteParameter7.SourceColumn = "Description";
            sqLiteParameter8.ParameterName = "@Url";
            sqLiteParameter8.SourceColumn = "Url";
            sqLiteParameter9.ParameterName = "@Ports";
            sqLiteParameter9.SourceColumn = "Ports";
            sqLiteParameter10.ParameterName = "@NetworkId";
            sqLiteParameter10.SourceColumn = "NetworkId";
            this.sqliteInsertCommand2.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter7,
            sqLiteParameter8,
            sqLiteParameter9,
            sqLiteParameter10});
            // 
            // sqliteUpdateCommand2
            // 
            this.sqliteUpdateCommand2.CommandText = resources.GetString("sqliteUpdateCommand2.CommandText");
            this.sqliteUpdateCommand2.Connection = this.sqLiteConnection1;
            sqLiteParameter11.ParameterName = "@Description";
            sqLiteParameter11.SourceColumn = "Description";
            sqLiteParameter12.ParameterName = "@Url";
            sqLiteParameter12.SourceColumn = "Url";
            sqLiteParameter13.ParameterName = "@Ports";
            sqLiteParameter13.SourceColumn = "Ports";
            sqLiteParameter14.ParameterName = "@NetworkId";
            sqLiteParameter14.SourceColumn = "NetworkId";
            sqLiteParameter15.ParameterName = "@Original_Id";
            sqLiteParameter15.SourceColumn = "Id";
            sqLiteParameter15.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter16.ParameterName = "@Original_Description";
            sqLiteParameter16.SourceColumn = "Description";
            sqLiteParameter16.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter17.ParameterName = "@Original_Url";
            sqLiteParameter17.SourceColumn = "Url";
            sqLiteParameter17.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter18.ParameterName = "@IsNull_Ports";
            sqLiteParameter18.SourceColumn = "Ports";
            sqLiteParameter18.SourceColumnNullMapping = true;
            sqLiteParameter18.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter19.ParameterName = "@Original_Ports";
            sqLiteParameter19.SourceColumn = "Ports";
            sqLiteParameter19.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter20.ParameterName = "@Original_NetworkId";
            sqLiteParameter20.SourceColumn = "NetworkId";
            sqLiteParameter20.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteUpdateCommand2.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter11,
            sqLiteParameter12,
            sqLiteParameter13,
            sqLiteParameter14,
            sqLiteParameter15,
            sqLiteParameter16,
            sqLiteParameter17,
            sqLiteParameter18,
            sqLiteParameter19,
            sqLiteParameter20});
            // 
            // sqliteDeleteCommand2
            // 
            this.sqliteDeleteCommand2.CommandText = resources.GetString("sqliteDeleteCommand2.CommandText");
            this.sqliteDeleteCommand2.Connection = this.sqLiteConnection1;
            sqLiteParameter21.ParameterName = "@Original_Id";
            sqLiteParameter21.SourceColumn = "Id";
            sqLiteParameter21.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter22.ParameterName = "@Original_Description";
            sqLiteParameter22.SourceColumn = "Description";
            sqLiteParameter22.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter23.ParameterName = "@Original_Url";
            sqLiteParameter23.SourceColumn = "Url";
            sqLiteParameter23.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter24.ParameterName = "@IsNull_Ports";
            sqLiteParameter24.SourceColumn = "Ports";
            sqLiteParameter24.SourceColumnNullMapping = true;
            sqLiteParameter24.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter25.ParameterName = "@Original_Ports";
            sqLiteParameter25.SourceColumn = "Ports";
            sqLiteParameter25.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter26.ParameterName = "@Original_NetworkId";
            sqLiteParameter26.SourceColumn = "NetworkId";
            sqLiteParameter26.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteDeleteCommand2.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter21,
            sqLiteParameter22,
            sqLiteParameter23,
            sqLiteParameter24,
            sqLiteParameter25,
            sqLiteParameter26});
            // 
            // serverDataAdapter
            // 
            this.serverDataAdapter.DeleteCommand = this.sqliteDeleteCommand2;
            this.serverDataAdapter.InsertCommand = this.sqliteInsertCommand2;
            this.serverDataAdapter.SelectCommand = this.sqliteSelectCommand2;
            this.serverDataAdapter.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "Servers", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("Id", "Id"),
                        new System.Data.Common.DataColumnMapping("Description", "Description"),
                        new System.Data.Common.DataColumnMapping("Url", "Url"),
                        new System.Data.Common.DataColumnMapping("Ports", "Ports"),
                        new System.Data.Common.DataColumnMapping("NetworkId", "NetworkId")})});
            this.serverDataAdapter.UpdateCommand = this.sqliteUpdateCommand2;
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
            this.Controls.Add(this.settingsTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ServerSettingsDialog";
            this.Text = "Servers";
            this.serverGroupBox.ResumeLayout(false);
            this.serverGroupBox.PerformLayout();
            this.networkGroupBox.ResumeLayout(false);
            this.networkGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ircSettingsDataSet1)).EndInit();
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
        private System.Data.SQLite.SQLiteCommand sqliteSelectCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteInsertCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteUpdateCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteDeleteCommand1;
        private System.Data.SQLite.SQLiteDataAdapter networkDataAdapter;
        private System.Data.SQLite.SQLiteConnection sqLiteConnection1;
        private IrcSettingsDataSet ircSettingsDataSet1;
        private System.Data.SQLite.SQLiteCommand sqliteSelectCommand2;
        private System.Data.SQLite.SQLiteCommand sqliteInsertCommand2;
        private System.Data.SQLite.SQLiteCommand sqliteUpdateCommand2;
        private System.Data.SQLite.SQLiteCommand sqliteDeleteCommand2;
        private System.Data.SQLite.SQLiteDataAdapter serverDataAdapter;
    }
}