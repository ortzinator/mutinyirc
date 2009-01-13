namespace OrtzIRC.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using FlamingIRC;
    using System.ComponentModel;

    public partial class NickListBox : ListBox
    {
        /// <summary>
        /// The Color Ops' nicks should be displayed as
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color OpColor { get; set; }

        /// <summary>
        /// The Color voices' nicks should be displayed as
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color VoiceColor { get; set; }

        /// <summary>
        /// The Color regular users' nicks should be displayed as
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color RegularUserColor { get; set; }

        public NickListBox()
        {
            InitializeComponent();

            this.Sorted = true;
            this.DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.Items.Count > 0)
            {
                e.DrawBackground();

                Brush fore = Brushes.Black;

                var cur = this.Items[e.Index] as User;

                switch (cur.Prefix)
                {
                    case '+':
                        fore = SystemBrushes.FromSystemColor(VoiceColor);
                        break;
                    case '@':
                        fore = SystemBrushes.FromSystemColor(OpColor);
                        break;
                    default:
                        fore = SystemBrushes.FromSystemColor(RegularUserColor);
                        break;
                }

                e.Graphics.DrawString(cur.ToString(), e.Font, fore, e.Bounds, StringFormat.GenericDefault);

                e.DrawFocusRectangle();
            }
            else
            {
                base.OnDrawItem(e);
            }
        }
    }
}
