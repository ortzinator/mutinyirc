namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;

    public class ChannelNodeContextMenu : ContextMenuStrip
    {
        public event EventHandler LeaveClick;

        public ChannelNodeContextMenu()
        {
            Items.Add("Leave").Click += delegate { LeaveClick.Fire(this, new EventArgs()); };
        }
    }
}
