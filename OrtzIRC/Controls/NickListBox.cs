using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;

namespace OrtzIRC.Controls
{
    public partial class NickListBox : ListBox
    {
        private List<string> _nickList;
        private delegate void ResetNicksDelegate();
        private delegate void AddNickDelegate(string nick);

        public NickListBox()
        {
            InitializeComponent();
            //this.DataSource = _nickList;
            this.Sorted = true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public void FillNicks(string[] nicks)
        {
            _nickList = new List<string>(nicks);
        }

        public void AddNick(string nick)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddNickDelegate(AddNick), new object[] { nick });
            }
            else
            {
                if (Rfc2812Util.IsValidNick(nick))
                {
                    //_nickList.Add(nick);
                    this.Items.Add(nick);
                    //this.Sort();
                }
            }
        }

        public void ResetNicks()
        {

            if (this.InvokeRequired)
            {
                this.Invoke(new ResetNicksDelegate(ResetNicks));
            }
            else
            {
                //_nickList = new List<string>();
                this.Items.Clear();
            }

        }
    }
}
