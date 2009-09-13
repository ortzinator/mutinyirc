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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSettingsDialog));
            System.Data.SQLite.SQLiteParameter sqLiteParameter1 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter2 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter3 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter4 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter5 = new System.Data.SQLite.SQLiteParameter();
            System.Data.SQLite.SQLiteParameter sqLiteParameter6 = new System.Data.SQLite.SQLiteParameter();
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
            this.sqLiteConnection1 = new System.Data.SQLite.SQLiteConnection();
            this.allServersCommand = new System.Data.SQLite.SQLiteCommand();
            this.sqliteSelectCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteInsertCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteUpdateCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqliteDeleteCommand1 = new System.Data.SQLite.SQLiteCommand();
            this.sqLiteDataAdapter1 = new System.Data.SQLite.SQLiteDataAdapter();
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
            // 
            // sqLiteConnection1
            // 
            this.sqLiteConnection1.ConnectionString = "data source=C:\\Users\\Brian\\Documents\\SVN\\OrtzIRC\\OrtzIRC\\bin\\x86\\Debug\\ircsetting" +
                "s.db";
            this.sqLiteConnection1.DefaultTimeout = 30;
            // 
            // allServersCommand
            // 
            this.allServersCommand.CommandText = resources.GetString("allServersCommand.CommandText");
            this.allServersCommand.Connection = this.sqLiteConnection1;
            // 
            // sqliteSelectCommand1
            // 
            this.sqliteSelectCommand1.CommandText = "SELECT        networks.*\r\nFROM            networks";
            this.sqliteSelectCommand1.Connection = this.sqLiteConnection1;
            // 
            // sqliteInsertCommand1
            // 
            this.sqliteInsertCommand1.CommandText = "INSERT INTO [networks] ([name]) VALUES (@name)";
            this.sqliteInsertCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter1.ParameterName = "@name";
            sqLiteParameter1.SourceColumn = "name";
            this.sqliteInsertCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter1});
            // 
            // sqliteUpdateCommand1
            // 
            this.sqliteUpdateCommand1.CommandText = "UPDATE [networks] SET [name] = @name WHERE (([id] = @Original_id) AND ([name] = @" +
                "Original_name))";
            this.sqliteUpdateCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter2.ParameterName = "@name";
            sqLiteParameter2.SourceColumn = "name";
            sqLiteParameter3.ParameterName = "@Original_id";
            sqLiteParameter3.SourceColumn = "id";
            sqLiteParameter3.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter4.ParameterName = "@Original_name";
            sqLiteParameter4.SourceColumn = "name";
            sqLiteParameter4.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteUpdateCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter2,
            sqLiteParameter3,
            sqLiteParameter4});
            // 
            // sqliteDeleteCommand1
            // 
            this.sqliteDeleteCommand1.CommandText = "DELETE FROM [networks] WHERE (([id] = @Original_id) AND ([name] = @Original_name)" +
                ")";
            this.sqliteDeleteCommand1.Connection = this.sqLiteConnection1;
            sqLiteParameter5.ParameterName = "@Original_id";
            sqLiteParameter5.SourceColumn = "id";
            sqLiteParameter5.SourceVersion = System.Data.DataRowVersion.Original;
            sqLiteParameter6.ParameterName = "@Original_name";
            sqLiteParameter6.SourceColumn = "name";
            sqLiteParameter6.SourceVersion = System.Data.DataRowVersion.Original;
            this.sqliteDeleteCommand1.Parameters.AddRange(new System.Data.SQLite.SQLiteParameter[] {
            sqLiteParameter5,
            sqLiteParameter6});
            // 
            // sqLiteDataAdapter1
            // 
            this.sqLiteDataAdapter1.DeleteCommand = this.sqliteDeleteCommand1;
            this.sqLiteDataAdapter1.InsertCommand = this.sqliteInsertCommand1;
            this.sqLiteDataAdapter1.SelectCommand = this.sqliteSelectCommand1;
            this.sqLiteDataAdapter1.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "networks", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("id", "id"),
                        new System.Data.Common.DataColumnMapping("name", "name")})});
            this.sqLiteDataAdapter1.UpdateCommand = this.sqliteUpdateCommand1;
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
        private System.Data.SQLite.SQLiteConnection sqLiteConnection1;
        private System.Data.SQLite.SQLiteCommand allServersCommand;
        private System.Data.SQLite.SQLiteCommand sqliteSelectCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteInsertCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteUpdateCommand1;
        private System.Data.SQLite.SQLiteCommand sqliteDeleteCommand1;
        private System.Data.SQLite.SQLiteDataAdapter sqLiteDataAdapter1;




    }
}