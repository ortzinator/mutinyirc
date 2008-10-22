namespace OrtzIRC
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.Plugins;
    using OrtzIRC.Resources;

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

            commandTextBox.CommandEntered += new EventHandler<DataEventArgs<string>>(commandTextBox_CommandEntered);
        }

        private void HookupEvents()
        {
            this.server.Registered += ParentServer_OnRegistered;
            this.server.PublicMessage += ParentServer_OnPublicMessage;
            this.server.UserAction += ParentServer_OnAction;
            this.server.JoinSelf += ParentServer_OnJoinSelf;
            this.server.JoinOther += ParentServer_OnJoinOther;
            this.server.Part += ParentServer_OnPart;
            this.server.ConnectFailed += Server_OnConnectFail;
            this.server.PrivateNotice += ParentServer_OnPrivateNotice;
            this.server.GotTopic += ParentServer_OnGotTopic;
            this.server.RawMessageReceived += ParentServer_OnRawMessageReceived;
            this.server.ErrorMessageRecieved += ParentServer_OnError;
            this.server.Kick += ParentServer_OnKick;
            this.server.Connecting += Server_Connecting;

        }

        private void UnhookEvents()
        {
            this.server.Registered -= ParentServer_OnRegistered;
            this.server.PublicMessage -= ParentServer_OnPublicMessage;
            this.server.UserAction -= ParentServer_OnAction;
            this.server.JoinSelf -= ParentServer_OnJoinSelf;
            this.server.JoinOther -= ParentServer_OnJoinOther;
            this.server.Part -= ParentServer_OnPart;
            this.server.ConnectFailed -= Server_OnConnectFail;
            this.server.PrivateNotice -= ParentServer_OnPrivateNotice;
            this.server.GotTopic -= ParentServer_OnGotTopic;
            this.server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            this.server.ErrorMessageRecieved -= ParentServer_OnError;
            this.server.Kick -= ParentServer_OnKick;
            this.server.Connecting -= Server_Connecting;
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            PluginManager.Instance.ParseCommand(this.Server, e.Data);
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            this.serverOutputBox.AppendLine(ServerStrings.ConnectingMessage.With(this.server.URI, this.server.Port));
        }

        private void ParentServer_OnKick(object sender, KickEventArgs e)
        {
            e.Channel.UserKick(e.User, e.Kickee, e.Reason);
        }

        private void ParentServer_OnPart(object sender, PartEventArgs e)
        {
            e.Channel.UserPart(e.User, e.Reason);
        }

        private void ParentServer_OnJoinOther(object sender, OrtzIRC.Common.DoubleDataEventArgs<User, Channel> e)
        {
            e.Second.UserJoin(e.First);
        }

        private void ParentServer_OnError(object sender, ErrorMessageEventArgs a)
        {
            this.serverOutputBox.AppendLine(a.Code.ToString() + " " + a.Message);
        }

        private void ParentServer_OnRawMessageReceived(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            //this.serverOutputBox.AddLine(message);
        }

        private void ParentServer_OnGotTopic(Channel chan, string topic)
        {
            chan.ShowTopic(topic);
        }

        private void ParentServer_OnPrivateNotice(object sender, PrivateMessageEventArgs e)
        {
            this.serverOutputBox.AppendLine("-" + e.User.Nick + ":" + e.Message + "-");
        }

        private void Server_OnConnectFail(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            this.serverOutputBox.AppendLine(ServerStrings.ConnectionFailedMessage.With(e.Data));
        }

        private void ParentServer_OnAction(object sender, ChannelMessageEventArgs e)
        {
            e.Channel.NewAction(e.User, e.Message);
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

        private void ParentServer_OnJoinSelf(object sender, OrtzIRC.Common.DataEventArgs<Channel> e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ChannelForm newChan = new ChannelForm(e.Data, this.server);
                newChan.MdiParent = this.MdiParent;
                newChan.Show();
                newChan.AddLine("Joined: " + e.Data.Name);
            });
        }

        private void ParentServer_OnPublicMessage(object sender, ChannelMessageEventArgs e)
        {
            e.Channel.NewMessage(e.User, e.Message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (this.server.IsConnected)
            {
                DialogResult result = MessageBox.Show(ServerStrings.WarnDisconnect, CommonStrings.DialogCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    this.server.Disconnect("OrtzIRC (pre-alpa) - http://code.google.com/p/ortzirc/"); //TODO: Pick random message from user-defined list of quit messages
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
