using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;

namespace OrtzIRC.Controls
{
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
            e.DrawBackground();

            Brush fore = Brushes.Black;

            var cur = this.Items[e.Index] as Nick;

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
            
            //base.OnDrawItem(e);
        }
    }
}
