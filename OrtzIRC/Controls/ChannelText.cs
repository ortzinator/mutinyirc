using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace OrtzIRC.Controls
{
    public partial class ChannelText : RichTextBox
    {
        private delegate void AppendLineHandler(string line);

        public ChannelText()
        {
            InitializeComponent();
            this.ReadOnly = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public void AddLine(string line)
        {
            if (this.InvokeRequired)
            {
                AppendLineHandler d = new AppendLineHandler(AddLine);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                DateTime now = DateTime.Now;
                this.Text += "\n" + now.ToString("T",
                  System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")) + " " + line.Trim();
                ScrollToBottom();
            }
        }

        public void ScrollToBottom()
        {
            this.SelectionStart = this.Text.Length;
            this.SelectionLength = 0;
            this.ScrollToCaret();
        }
    }
}
