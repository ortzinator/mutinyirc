namespace OrtzIRC
{
    using System.Drawing;
    using System.Windows.Forms;
    using FlamingIRC;
    using System.ComponentModel;
    using System;

    public partial class NickListBox : ListBox
    {
        private UserList userList;

        public NickListBox()
        {
            InitializeComponent();

            Sorted = true;
            DrawMode = DrawMode.OwnerDrawFixed;
            OpColor = Color.Black;
            VoiceColor = Color.Black;
            RegularUserColor = Color.Black;
        }

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

        [Browsable(false)]
        public UserList UserList
        {
            get { return userList; }
            set
            {
                userList = value;
                if (value != null)
                    userList.Updated += userList_Updated;
            }
        }

        private void userList_Updated(object sender, System.EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action(UpdateUserList));
            else
                UpdateUserList();
        }

        private void UpdateUserList()
        {
            lock (userList)
            {
                Items.Clear();

                SuspendLayout();
                foreach (User nick in userList)
                    Items.Add(nick);
                ResumeLayout();
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count > 0)
            {
                e.DrawBackground();

                Brush fore = Brushes.Black;

                User cur = (User)Items[e.Index];

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
