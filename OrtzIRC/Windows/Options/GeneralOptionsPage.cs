namespace OrtzIRC
{
    public partial class GeneralOptionsPage : OptionsPage
    {
        public GeneralOptionsPage()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get
            {
                return "General";
            }
        }

        public override System.Drawing.Image Image
        {
            get
            {
                return Resources.Icons.Gear;
            }
        }
    }
}
