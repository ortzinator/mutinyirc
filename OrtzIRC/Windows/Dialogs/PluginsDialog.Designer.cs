namespace OrtzIRC
{
    partial class PluginsDialog
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
            this.pluginListView = new System.Windows.Forms.ListView();
            this.configureButton = new System.Windows.Forms.Button();
            this.aboutButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pluginListView
            // 
            this.pluginListView.Location = new System.Drawing.Point(12, 12);
            this.pluginListView.Name = "pluginListView";
            this.pluginListView.Size = new System.Drawing.Size(664, 435);
            this.pluginListView.TabIndex = 1;
            this.pluginListView.UseCompatibleStateImageBehavior = false;
            // 
            // configureButton
            // 
            this.configureButton.Location = new System.Drawing.Point(601, 453);
            this.configureButton.Name = "configureButton";
            this.configureButton.Size = new System.Drawing.Size(75, 23);
            this.configureButton.TabIndex = 2;
            this.configureButton.Text = "Configure";
            this.configureButton.UseVisualStyleBackColor = true;
            // 
            // aboutButton
            // 
            this.aboutButton.Location = new System.Drawing.Point(520, 453);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(75, 23);
            this.aboutButton.TabIndex = 3;
            this.aboutButton.Text = "About";
            this.aboutButton.UseVisualStyleBackColor = true;
            // 
            // PluginsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 488);
            this.Controls.Add(this.pluginListView);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.configureButton);
            this.Name = "PluginsDialog";
            this.Text = "Plugins - OrtzIRC";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView pluginListView;
        private System.Windows.Forms.Button configureButton;
        private System.Windows.Forms.Button aboutButton;

    }
}