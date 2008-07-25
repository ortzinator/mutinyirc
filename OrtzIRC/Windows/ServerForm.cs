using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;

namespace OrtzIRC
{
    public partial class ServerForm : Form
    {
        private Connection _con;
        private string _nick;
        private Dictionary<string, ChannelForm> _channels;
        delegate void SetTextCallback(string text);

        public ServerForm()
        {
            InitializeComponent();

            //TODO: Get nick from settings
            _nick = "Ortz";
            ConnectionArgs args = new ConnectionArgs(_nick, "irc.gamesurge.net");

            _con = new Connection(args, false, false);

            _con.Listener.OnJoin += new JoinEventHandler(OnJoin);
            _con.Listener.OnPublic += new PublicMessageEventHandler(OnPublic);
            _con.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);

            this.FormClosing += new FormClosingEventHandler(ServerForm_FormClosing);

            try
            {
                _con.Connect();
            }
            catch (Exception e)
            {
                ServerOutLine("Could not connect to server: " + e.Message);
            }
        }

        void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_con.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _con.Disconnect("Quitan");
                    //TODO: Close all channel windows
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void ServerOutLine(string line)
        {

            if (this.serverOutputBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ServerOutLine);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                serverOutputBox.Text += "\n" + line.Trim();
            }

        }

        private void OnJoin(UserInfo user, string channel)
        {
            if (user.Nick == _nick)
            {
                ServerOutLine("Joined: " + channel);
            }
            else
            {
                ServerOutLine("<<" + user.Nick + ">> has joined " + channel);
            }
        }

        private void OnPublic(UserInfo user, string channel, string message)
        {
            ServerOutLine("<" + user.Nick + "> " + message);
        }

        private void OnRegistered()
        {
            _con.Sender.Join("#luahelp");
        }
    }
}