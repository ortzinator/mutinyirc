namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;

    public partial class EditServerDialog : Form
    {
        public EditServerDialog()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}