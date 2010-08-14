namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public partial class CommandTextBox : TextBox
    {
        private int _historyIndex;

        private bool keyPressHandled = false;

        public CommandTextBox()
        {
            InitializeComponent();

            CmdHistory = new List<string> { Capacity = 40 };
        }

        public List<string> CmdHistory { get; private set; }
        public event EventHandler<DataEventArgs<string>> CommandEntered;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            keyPressHandled = false;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        Text = CmdHistory[_historyIndex];
                        keyPressHandled = true;
                    }
                    break;
                case Keys.Down:
                    if (_historyIndex == CmdHistory.Count && Text.Trim() != string.Empty)
                    {
                        CmdHistory.Add(Text);
                        _historyIndex = CmdHistory.Count;
                        Clear();
                        keyPressHandled = true;
                    }
                    else if (_historyIndex == CmdHistory.Count - 1)
                    {
                        _historyIndex++;
                        Clear();
                        keyPressHandled = true;
                    }
                    else if (_historyIndex < CmdHistory.Count)
                    {
                        _historyIndex++;
                        Text = CmdHistory[_historyIndex];
                        keyPressHandled = true;
                    }
                    break;
                case Keys.Enter:
                    if (Text.Trim() != string.Empty)
                    {
                        if (_historyIndex != CmdHistory.Count)
                        {
                            CmdHistory.RemoveAt(_historyIndex);
                        }
                        CmdHistory.Add(Text);

                        CommandEntered.Fire(this, new DataEventArgs<string>(Text.Trim()));

                        Clear();
                        _historyIndex = CmdHistory.Count;
                        keyPressHandled = true;
                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (keyPressHandled)
            {
                e.Handled = true;
            }

            base.OnKeyPress(e);
        }
    }
}
