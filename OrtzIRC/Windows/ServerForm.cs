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

                    // todo - unhook events
                }

                this.server = value;

                this.server.Connecting += Server_Connecting;
            }
        }    

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm()
        {
            InitializeComponent();

            this.server.Registered += new RegisteredEventHandler(ParentServer_OnRegistered);
            this.server.PublicMessage += new Server_ChannelMessageEventHandler(ParentServer_OnPublicMessage);
            this.server.Action += new Server_ChannelMessageEventHandler(ParentServer_OnAction);
            this.server.JoinSelf += new EventHandler<DataEventArgs<Channel>>(ParentServer_OnJoinSelf);
            this.server.JoinOther += ParentServer_OnJoinOther;
            this.server.Part += new Server_ChannelMessageEventHandler(ParentServer_OnPart);
            this.server.ConnectFail += new Server_ConnectFailedEventHandler(ParentServer_OnConnectFail);
            this.server.PrivateNotice += new Server_PrivateNoticeEventHandler(ParentServer_OnPrivateNotice);
            this.server.GotTopic += new Server_TopicRequestEventHandler(ParentServer_OnGotTopic);
            this.server.RawMessageReceived += new RawMessageReceivedEventHandler(ParentServer_OnRawMessageReceived);
            this.server.Error += new ErrorMessageEventHandler(ParentServer_OnError);
            this.server.Kick += new Server_KickEventHandler(ParentServer_OnKick);

            this.commandTextBox.Focus();
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            this.serverOutputBox.AddLine(ServerStrings.ConnectingDisplayMessage.With(this.server.URI, this.server.Port));
        }

        void ParentServer_OnKick(Nick nick, Channel chan, string kickee, string reason)
        {
            chan.UserKick(nick, kickee, reason);
        }

        void ParentServer_OnPart(Nick nick, Channel chan, string message)
        {
            chan.UserPart(nick, message);
        }

        private void ParentServer_OnJoinOther(object sender, DoubleDataEventArgs<Nick, Channel> e)
        {
            e.Second.UserJoin(e.First);
        }

        void ParentServer_OnError(ReplyCode code, string message)
        {
            this.serverOutputBox.AddLine(code.ToString() + " " + message);
        }

        void ParentServer_OnRawMessageReceived(string message)
        {
            //this.serverOutputBox.AddLine(message);
        }

        void ParentServer_OnGotTopic(Channel chan, string topic)
        {
            chan.ShowTopic(topic);
        }

        void ParentServer_OnPrivateNotice(Nick nick, string notice)
        {
            this.serverOutputBox.AddLine("-" + nick.Name + ":" + notice + "-");
        }

        void ParentServer_OnConnectFail(string message)
        {
            this.serverOutputBox.AddLine("Could not connect");
        }

        void ParentServer_OnAction(Nick nick, Channel chan, string message)
        {
            chan.NewAction(nick, message);
        }

        void ParentServer_OnRegistered()
        {
            this.server.Connection.Sender.Join("##csharp");

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

        void ParentServer_OnPublicMessage(Nick nick, Channel chan, string message)
        {
            chan.NewMessage(nick, message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.server.Connection.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.server.Connection.Disconnect("Quitan");
                }
                else
                {
                    e.Cancel = true;
                }
            }

            base.OnFormClosing(e);
        }
    }
}