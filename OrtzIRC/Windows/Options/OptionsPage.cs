namespace OrtzIRC
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class OptionsPage : UserControl
    {
        public OptionsPage()
        {
            InitializeComponent();
        }

        public new virtual string Text
        {
            get { return GetType().Name; }
        }

        public virtual Image Image
        {
            get { return null; }
        }

        public virtual void OnSetActive() { }

        public virtual void OnApply() { }
    }
}
