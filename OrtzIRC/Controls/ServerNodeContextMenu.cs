namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;

    public class ServerNodeContextMenu : ContextMenuStrip
    {
        public event EventHandler DisconnectClick;

        public ServerNodeContextMenu()
        {
            Items.Add("Disconnect...").Click += delegate { DisconnectClick.Fire(this, new EventArgs()); };
        }
    }
}
