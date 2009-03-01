namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;

    public partial class NewServerDialog : Form
    {
        public NewServerDialog()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}