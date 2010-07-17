namespace OrtzIRC
{
    using System.Windows.Forms;

    public partial class InputDialog : Form
    {
        public string Input { get; set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string windowCaption, string inputCaption, string confirmText)
        {
            InitializeComponent();
            Text = windowCaption;
            captionLabel.Text = inputCaption;
            confirmButton.Text = confirmText;
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void connectButton_Click(object sender, System.EventArgs e)
        {
            Input = inputTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
