using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC.Intellisense
{
    public partial class AutoCompleteForm : Form
    {
        public AutoCompleteForm(Point position)
        {
            InitializeComponent();

            this.Top = position.Y + 5;
            this.Left = position.X + 5;
        }
    }
}
