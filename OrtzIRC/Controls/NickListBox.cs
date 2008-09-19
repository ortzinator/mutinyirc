namespace OrtzIRC.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using FlamingIRC;

    public partial class NickListBox : ListBox
    {
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
                    case (char)0:
                        fore = Brushes.Gray;
                        break;
                    case '+':
                        fore = Brushes.Blue;
                        break;
                }

                e.Graphics.DrawString(cur.NamesLiteral, e.Font, fore, e.Bounds, StringFormat.GenericDefault);

                e.DrawFocusRectangle();
            }
            else
            {
                base.OnDrawItem(e);
            }
        }
    }
}
