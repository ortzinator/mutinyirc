namespace OrtzIRC.Dialogs
{
    using System;
    using System.Windows.Forms;
    using OrtzIRC.Common;

    public partial class ServerDialog : Form
    {
        private IRCSettingsManager serverManager;

        public ServerDialog()
        {
            serverManager = IRCSettingsManager.Instance;
            FormClosing += ServerDialog_FormClosing;
            InitializeComponent();
        }

        private void ServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Settings.Default.Save();
        }

        private void ServerDialog_Load(object sender, EventArgs e)
        {
            //foreach (Server server in Settings.Default.ServerList)
            {
                //serverListBox.Items.Add(server.Description);
            }
            //serverListBox.Items.Add("goo");
            //serverListBox.Items.Add("goo");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Server temp = new Server("irc.gamesurge.net", "Hello new server", 6667);

            //Settings.Default.ServerList.Add(temp);
        }
    }
}