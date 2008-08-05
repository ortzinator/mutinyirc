using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace OrtzIRC.Controls
{
    public partial class ChannelText : RichTextBox
    {
        private delegate void AppendLineHandler(string line);

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
            if (this.InvokeRequired)
            {
                AppendLineHandler d = new AppendLineHandler(AppendLine);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                DateTime now = DateTime.Now;
                this.Text += "\n" + now.ToString("T",
                  CultureInfo.CreateSpecificCulture("es-ES")) + " " + line.Trim();
                this.SelectionStart = this.Text.Length;
                this.SelectionLength = 0;
            }
        }
    }
}
