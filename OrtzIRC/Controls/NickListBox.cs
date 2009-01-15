namespace OrtzIRC.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using FlamingIRC;
    using System.ComponentModel;

    public partial class NickListBox : ListBox
    {
        /// <summary>
        /// The Color Ops' nicks should be displayed as
        /// </summary>
        public Color OpColor { get; set; }

        /// <summary>
        /// The Color voices' nicks should be displayed as
        /// </summary>
        public Color VoiceColor { get; set; }

        /// <summary>
        /// The Color regular users' nicks should be displayed as
        /// </summary>
        public Color RegularUserColor { get; set; }

        private UserList userList;

        [Browsable(false)]
        public UserList UserList
        {
            get { return userList; }
            set
            {
                userList = value;
                if (value != null)
                    userList.Updated += new System.EventHandler(userList_Updated);
            }
        }

        private void userList_Updated(object sender, System.EventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                Items.Clear();

                foreach (User nick in userList)
                {
                    Items.Add(nick);
                }
            });
        }

        public NickListBox()
        {
            InitializeComponent();

            this.Sorted = true;
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.OpColor = Color.Black;
            this.VoiceColor = Color.Black;
            this.RegularUserColor = Color.Black;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.Items.Count > 0)
            {
                e.DrawBackground();

                Brush fore = Brushes.Black;

                var cur = this.Items[e.Index] as User;

                switch (cur.Prefix)
                {
                    case '+':
                        fore = new SolidBrush(VoiceColor);
                        break;
                    case '@':
                        fore = new SolidBrush(OpColor);
                        break;
                    default:
                        fore = new SolidBrush(RegularUserColor);
                        break;
                }

                e.Graphics.DrawString(cur.ToString(), e.Font, fore, e.Bounds, StringFormat.GenericDefault);

                e.DrawFocusRectangle();
            }
            else
            {
                base.OnDrawItem(e);
            }
        }
    }
}
