using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;
using System.Collections;

namespace OrtzIRC
{
    public partial class ServerForm : Form
    {
        internal Sharkbite.Irc.Connection Con
        {
            get;
            private set;
        }

        private string _nick;
        private Dictionary<string, ChannelForm> _channels = new Dictionary<string, ChannelForm>();

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm(ServerSettings settings)
        {
            InitializeComponent();

            this.Text = settings.URI;

            //TODO: Get nick from settings
            _nick = "OrtzIRC";
            ConnectionArgs args = new ConnectionArgs(_nick, settings.URI);

            Con = new Connection(args, false, false);

            Con.Listener.OnJoin += new JoinEventHandler(OnJoin);
            Con.Listener.OnPart += new PartEventHandler(OnPart);
            Con.Listener.OnPublic += new PublicMessageEventHandler(OnPublic);
            Con.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
            Con.Listener.OnNames += new NamesEventHandler(OnNames);
            Con.Listener.OnChannelModeChange += new ChannelModeChangeEventHandler(OnChannelModeChange);
            Con.Listener.OnError += new ErrorMessageEventHandler(OnError);

            this.FormClosing += new FormClosingEventHandler(ServerForm_FormClosing);

            try
            {
                Con.Connect();
            }
            catch (Exception e)
            {
                this.AppendLine("Could not connect to server: " + e.Message);
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Con.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Con.Disconnect("Quitan");
                    foreach (KeyValuePair<string, ChannelForm> chan in _channels)
                    {
                        chan.Value.Close();
                    }
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

        private void NewChannel(string channel)
        {
            ChannelForm newChan = new ChannelForm(this, channel);
            newChan.Text = channel;
            newChan.MdiParent = this.MdiParent;
            newChan.AppendLine("Joined: " + channel);
            _channels.Add(channel, newChan);
            newChan.Show();
            newChan.Focus();
        }

        private void JoinChannel(string channel)
        {
            Con.Sender.Join(channel);
        }

        private void OnPublic(UserInfo user, string channel, string message)
        {
            _channels[channel].AppendLine("<" + user.Nick + "> " + message);
        }

        private bool recievingNames = false;
        private void OnNames(string channel, string[] nicks, bool last)
        {
            if (!recievingNames)
            {
                _channels[channel].ResetNicks();
                recievingNames = true;
            }

            foreach (string nick in nicks)
            {
                _channels[channel].AddNick(nick);
            }

            if (last)
            {
                recievingNames = false;
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
                Con.Sender.Names(channel);
            }
        }

        private void OnPart(UserInfo user, string channel, string reason)
        {
            _channels[channel].AppendLine("<<" + user.Nick + ">> has parted " + channel);
            Con.Sender.Names(channel);
        }

        private void OnRegistered()
        {
            this.AppendLine("Connected to server");
            JoinChannel("#ortzirc");
        }

        private void OnChannelModeChange(UserInfo who, string channel, ChannelModeInfo[] modes, string raw)
        {
            _channels[channel].AppendLine("<<" + who.Nick + ">> sets mode (" + raw + ")");
            Con.Sender.Names(channel);
        }

        private void OnError(ReplyCode code, string message)
        {
            this.AppendLine(message);
        }

    }
}