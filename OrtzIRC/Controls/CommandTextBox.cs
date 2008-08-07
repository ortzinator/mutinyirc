using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC.Controls
{
    public delegate void CommandEnteredEventHandler(string command);

    public partial class CommandTextBox : TextBox
    {
        public List<string> CmdHistory { get; private set; }
        private int _historyIndex;

        public event CommandEnteredEventHandler OnCommandEntered;

        public CommandTextBox()
        {
            InitializeComponent();

            CmdHistory = new List<string>();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        this.Text = CmdHistory[_historyIndex];
                    }
                    break;
                case Keys.Down:
                    if (_historyIndex == CmdHistory.Count && this.Text.Trim() != string.Empty)
                    {
                        CmdHistory.Add(this.Text);
                        _historyIndex = CmdHistory.Count;
                        this.Clear();
                    }
                    else if (_historyIndex == CmdHistory.Count - 1)
                    {
                        _historyIndex++;
                        this.Clear();
                    }
                    else if (_historyIndex < CmdHistory.Count)
                    {
                        _historyIndex++;
                        this.Text = CmdHistory[_historyIndex];
                    }
                    break;
                case Keys.Enter:
                    if (this.Text.Trim() != string.Empty)
                    {
                        if (_historyIndex != CmdHistory.Count)
                        {
                            CmdHistory.RemoveAt(_historyIndex);
                        }
                        CmdHistory.Add(this.Text); 

                        if (OnCommandEntered != null)
                            OnCommandEntered(this.Text.Trim());

                        this.Clear();
                        _historyIndex = CmdHistory.Count;
                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
