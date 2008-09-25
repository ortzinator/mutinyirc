namespace OrtzIRC.Controls
{
    using System;
    using System.Windows.Forms;
    using System.Drawing;

    public partial class IrcTextBox : RichTextBox
    {
        private Color[] IrcColors = new Color[]
			{
			    Color.White,
			    Color.Black,
			    Color.DarkBlue,
			    Color.Green,
			    Color.Red,
			    Color.Maroon,
			    Color.Purple,
			    Color.Orange,
			    Color.Yellow,
			    Color.LimeGreen,
			    Color.Aqua,
			    Color.SkyBlue,
			    Color.Blue,
			    Color.Pink,
			    Color.DarkGray,
			    Color.Gray
			};

        public IrcTextBox()
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

        private new void AppendText(string line)
        {
            //TODO: Color parsing - http://www.mirc.co.uk/help/color.txt

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                switch (c)
                {
                    case (char)2: //bold
                        if (SelectionFont.Style == FontStyle.Bold)
                            SetFontStyle(FontStyle.Regular);
                        else
                            SetFontStyle(FontStyle.Bold);
                        break;
                    case ((char)3): //color
                        break;
                    case ((char)37): //underline
                        if (SelectionFont.Style == FontStyle.Underline)
                            SetFontStyle(FontStyle.Regular);
                        else
                            SetFontStyle(FontStyle.Underline);
                        break;
                    case ((char)26): //reverse (colors)
                        if (SelectionColor == BackColor)
                        {
                            SetColor(ForeColor);
                            SetBackColor(BackColor);
                        }
                        else
                        {
                            SetColor(BackColor);
                            SetBackColor(ForeColor);
                        }
                        break;
                    default:
                        base.AppendText(c.ToString());
                        break;
                }
            }
            SetFontStyle(FontStyle.Regular);
        }

        public void AppendLine(string line)
        {
            this.Invoke((MethodInvoker)delegate
            {
                DateTime now = DateTime.Now;

                this.AppendText("\n" + now.ToString("T",
                    System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")) + " ");
                this.AppendText(line.Trim());
                ScrollToBottom();
            });
        }

        private void SetColor(Color c)
        {
            SelectionColor = c;
        }

        private void SetBackColor(Color c)
        {
            SelectionBackColor = c;
        }

        private void SetFontStyle(FontStyle fs)
        {
            SelectionFont = new Font(Font, fs);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            this.SelectionStart = this.Text.Length;
            this.SelectionLength = 0;
            this.ScrollToCaret();
        }
    }
}
