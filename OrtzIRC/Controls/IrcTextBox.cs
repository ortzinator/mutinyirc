namespace OrtzIRC
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
			    Color.Gray,
			    Color.LightGray
			};

        public IrcTextBox()
        {
            InitializeComponent();
            ReadOnly = true;
            DetectUrls = false;
        }

        /// <summary>
        /// Add text from IRC which may contain colr codes and parse them.
        /// </summary>
        /// <param name="line">The text</param>
        private void AppendIrcText(string line)
        {
            //-- Color parsing: http://www.mirc.co.uk/help/color.txt

            int forecolor = 1;
            int backcolor = 0;

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
                        if (i + 1 == line.Length)
                        {
                            SetColor(ForeColor);
                            SetBackColor(BackColor);
                            break;
                        }

                        if (Char.IsDigit(line[i + 1]))
                        {
                            int offset = line.IndexOf(',', i, 5);
                            forecolor = Int32.Parse(line.Substring(i + 1, Char.IsDigit(line[i + 2]) ? 2 : 1));
                            if (offset != -1) // we have a back color as well
                                backcolor = Int32.Parse(line.Substring(offset + 1, Char.IsDigit(line[i + 4]) ? 2 : 1));
                            i += (forecolor >= 0 && forecolor < 10 ? 1 : 2);
                            i += (offset == -1 ? 0 : (backcolor >= 0 && backcolor < 10 ? 2 : 3));
                        }

                        if (forecolor <= IrcColors.Length)
                        {
                            SetColor(IrcColors[forecolor]);
                        }

                        if (backcolor <= IrcColors.Length)
                        {
                            SetBackColor(IrcColors[backcolor]);
                        }
                        break;
                    case ((char)31): //underline
                        if (SelectionFont.Style == FontStyle.Underline)
                            SetFontStyle(FontStyle.Regular);
                        else
                            SetFontStyle(FontStyle.Underline);
                        break;
                    case ((char)22): //reverse (colors)
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
                        AppendText(c.ToString());
                        break;
                }
            }
            SetFontStyle(FontStyle.Regular);
            SetColor(ForeColor);
            SetBackColor(BackColor);
        }

        public void AppendLine(string line)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLine), line);
            }
            else
            {
                DateTime now = DateTime.Now;

                AppendIrcText("\n" + now.ToString("T",
                    System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")) + " ");
                AppendIrcText(line.Trim());
                ScrollToBottom();
            }
        }

        public void AppendLine(string line, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(AppendLine), line, color);
            }
            else
            {
                SetColor(color);
                AppendText("\n" + line);
                SetColor(ForeColor);
                ScrollToBottom();
            }
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
            ResetSelection();
            ScrollToCaret();
        }

        private void ResetSelection()
        {
            SelectionStart = Text.Length;
            SelectionLength = 0;
        }

        //hack - might be a better way than using OnMouseUp...
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);

            string trimmed = SelectedText.Trim();

            if (trimmed.Length > 1)
            {
                Clipboard.SetText(trimmed, TextDataFormat.UnicodeText);
            }
            ResetSelection();
        }
    }
}
