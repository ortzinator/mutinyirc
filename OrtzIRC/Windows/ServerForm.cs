namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;
    using OrtzIRC.Common;
    using OrtzIRC.Resources;
    using Sharkbite.Irc;

    public partial class ServerForm : Form
    {
        private Server server;

        public Server Server
        {
            get { return this.server; }
            set
            {
                if (this.server != null)
                {
                    this.server.Disconnect();

                    this.UnhookEvents();
                }

                this.server = value;
                
                this.HookupEvents();
            }
        }    

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm()
        {
            InitializeComponent();

            this.commandTextBox.Focus();
        }

        private void HookupEvents()
        {
            this.server.Registered += ParentServer_OnRegistered;
            this.server.PublicMessage += ParentServer_OnPublicMessage;
            this.server.Action += ParentServer_OnAction;
            this.server.JoinSelf += ParentServer_OnJoinSelf;
            this.server.JoinOther += ParentServer_OnJoinOther;
            this.server.Part += ParentServer_OnPart;
            this.server.ConnectFail += Server_OnConnectFail;
            this.server.PrivateNotice += ParentServer_OnPrivateNotice;
            this.server.GotTopic += ParentServer_OnGotTopic;
            this.server.RawMessageReceived += ParentServer_OnRawMessageReceived;
            this.server.Error += ParentServer_OnError;
            this.server.Kick += ParentServer_OnKick;
            this.server.Connecting += Server_Connecting;
        }

        private void UnhookEvents()
        {
            this.server.Registered -= ParentServer_OnRegistered;
            this.server.PublicMessage -= ParentServer_OnPublicMessage;
            this.server.Action -= ParentServer_OnAction;
            this.server.JoinSelf -= ParentServer_OnJoinSelf;
            this.server.JoinOther -= ParentServer_OnJoinOther;
            this.server.Part -= ParentServer_OnPart;
            this.server.ConnectFail -= Server_OnConnectFail;
            this.server.PrivateNotice -= ParentServer_OnPrivateNotice;
            this.server.GotTopic -= ParentServer_OnGotTopic;
            this.server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            this.server.Error -= ParentServer_OnError;
            this.server.Kick -= ParentServer_OnKick;
            this.server.Connecting -= Server_Connecting;
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            this.serverOutputBox.AddLine(ServerStrings.ConnectingMessage.With(this.server.URI, this.server.Port));
        }

        private void ParentServer_OnKick(Nick nick, Channel chan, string kickee, string reason)
        {
            chan.UserKick(nick, kickee, reason);
        }

        private void ParentServer_OnPart(Nick nick, Channel chan, string message)
        {
            chan.UserPart(nick, message);
        }

        private void ParentServer_OnJoinOther(object sender, DoubleDataEventArgs<Nick, Channel> e)
        {
            e.Second.UserJoin(e.First);
        }

        private void ParentServer_OnError(ReplyCode code, string message)
        {
            this.serverOutputBox.AddLine(code.ToString() + " " + message);
        }

        private void ParentServer_OnRawMessageReceived(string message)
        {
            //this.serverOutputBox.AddLine(message);
        }

        private void ParentServer_OnGotTopic(Channel chan, string topic)
        {
            chan.ShowTopic(topic);
        }

        private void ParentServer_OnPrivateNotice(Nick nick, string notice)
        {
            this.serverOutputBox.AddLine("-" + nick.Name + ":" + notice + "-");
        }

        private void Server_OnConnectFail(object sender, DataEventArgs<string> e)
        {
            this.serverOutputBox.AddLine(ServerStrings.ConnectionFailedMessage.With(e.Data));
        }

        private void ParentServer_OnAction(Nick nick, Channel chan, string message)
        {
            chan.NewAction(nick, message);
        }

        private void ParentServer_OnRegistered()
        {
            this.server.Connection.Sender.Join("#ortzirc");

            this.Invoke((MethodInvoker)delegate
            {
                this.Text = "Status: " + this.server.UserNick + " on " + this.server.Description +
                " (" + this.server.URI + ":" + this.server.Port + ")";
            });
        }

        private void ParentServer_OnJoinSelf(object sender, DataEventArgs<Channel> e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ChannelForm newChan = new ChannelForm(e.Data, this.server);
                newChan.MdiParent = this.MdiParent;
                newChan.Show();
                newChan.AddLine("Joined: " + e.Data.Name);
            });
        }

        private void ParentServer_OnPublicMessage(Nick nick, Channel chan, string message)
        {
            chan.NewMessage(nick, message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (this.server.Connection.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.server.Connection.Disconnect("OrtzIRC (pre-alpa) - http://code.google.com/p/ortzirc/"); //TODO: Pick random message from user-defined list of quit messages
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
