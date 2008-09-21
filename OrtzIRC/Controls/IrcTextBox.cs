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

        public void AddLine(string line)
        {
            //TODO: Color parsing - http://www.mirc.co.uk/help/color.txt
            this.Invoke((MethodInvoker)delegate
            {
                DateTime now = DateTime.Now;

                for (int i = 0; i < line.Length; i++)
                {
                    switch (line[i])
                    {
                        case (char)2: //bold
                            SelectionFont = new Font(this.Font, FontStyle.Bold);
                            break;
                        default:
                            this.Text += "\n" + now.ToString("T",
                                                System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")) + " " + line.Trim();
                            ScrollToBottom();
                            break;
                    }
                }
            });
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
