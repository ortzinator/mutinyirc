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
        private Dictionary<string, ChannelForm> _channels = new Dictionary<string,ChannelForm>();
        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm(ServerSettings settings)
        {
            InitializeComponent();

            //TODO: Get nick from settings
            _nick = "Ortz";
            ConnectionArgs args = new ConnectionArgs(_nick, settings.URI);

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
                this.AppendLine("Could not connect to server: " + e.Message);
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

        private void AppendLine(string line)
        {

            if (this.serverOutputBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AppendLine);
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
                if (_channels.ContainsKey(channel))
                {
                    _channels[channel].AppendLine("Joined: " + channel);
                }
                else
                {
                    this.Invoke(new NewChannelCallback(NewChannel), new object[] { channel });
                    //this.NewChannel(channel);
                }

            }
            else
            {
                _channels[channel].AppendLine("<<" + user.Nick + ">> has joined " + channel);
            }
        }

        private void NewChannel(string channel)
        {
            ChannelForm newChan = new ChannelForm();
            newChan.Text = channel;
            newChan.MdiParent = this.MdiParent;
            newChan.AppendLine("Joined: " + channel);
            _channels.Add(channel, newChan);
            newChan.Show();
            newChan.Focus();
        }

        private void OnPublic(UserInfo user, string channel, string message)
        {
            _channels[channel].AppendLine("<" + user.Nick + "> " + message);
        }

        private void OnRegistered()
        {
            JoinChannel("#ortzirc");
        }

        private void JoinChannel(string channel)
        {
            _con.Sender.Join(channel);
        }
    }
}