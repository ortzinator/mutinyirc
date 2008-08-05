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
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
