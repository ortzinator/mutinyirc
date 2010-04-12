namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public class PmNodeContextMenu : ContextMenuStrip
    {
        public event EventHandler CloseClick;

        public PmNodeContextMenu()
        {
            Items.Add("Close").Click += delegate { CloseClick.Fire(this, EventArgs.Empty); };
        }
    }
}
