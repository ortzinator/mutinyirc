using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC.Controls
{
    public partial class ChannelText : RichTextBox
    {
        public ChannelText()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public void AppendLine(string line)
        {
            this.Text += "\n" + line.Trim();
        }
    }
}
