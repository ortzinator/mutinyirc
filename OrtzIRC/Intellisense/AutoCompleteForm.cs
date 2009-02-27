namespace OrtzIRC.Intellisense
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class AutoCompleteForm : Form
    {
        public AutoCompleteForm(Point position)
        {
            InitializeComponent();

            Top = position.Y + 5;
            Left = position.X + 5;
        }
    }
}
