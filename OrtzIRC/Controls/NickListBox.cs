using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC.Controls
{
    public partial class NickListBox : ListBox
    {
        private List<FlamingIRC.Nick> _nickList;

        public NickListBox()
        {
            InitializeComponent();
            this.DataSource = _nickList;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public void FillNicks()
        {

        }
    }
}
