namespace OrtzIRC
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.Resources;
    using OrtzIRC.PluginFramework;

    public partial class ServerForm : Form
    {
        private Server server;

        public ServerForm()
        {
            InitializeComponent();

            commandTextBox.Focus();

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;
        }

        public Server Server
        {
            get { return server; }
            set
            {
                if (server != null)
                {
                    server.Disconnect();
                    UnhookEvents();
                }

                server = value;
                HookupEvents();
            }
        }

        private void HookupEvents()
        {
            server.Registered += ParentServer_OnRegistered;
            server.ChannelMessaged += ParentServer_ChannelMessaged;
            server.UserAction += ParentServer_UserAction;
            server.JoinSelf += ParentServer_OnJoinSelf;
            server.JoinOther += ParentServer_OnJoinOther;
            server.Part += ParentServer_OnPart;
            server.ConnectFailed += Server_OnConnectFail;
            server.PrivateNotice += ParentServer_OnPrivateNotice;
            server.GotTopic += ParentServer_OnGotTopic;
            server.RawMessageReceived += ParentServer_OnRawMessageReceived;
            server.ErrorMessageRecieved += ParentServer_OnError;
            server.Kick += ParentServer_OnKick;
            server.Connecting += Server_Connecting;
            server.Disconnected += server_Disconnected;
        }

        private void UnhookEvents()
        {
            server.Registered -= ParentServer_OnRegistered;
            server.ChannelMessaged -= ParentServer_ChannelMessaged;
            server.UserAction -= ParentServer_UserAction;
            server.JoinSelf -= ParentServer_OnJoinSelf;
            server.JoinOther -= ParentServer_OnJoinOther;
            server.Part -= ParentServer_OnPart;
            server.ConnectFailed -= Server_OnConnectFail;
            server.PrivateNotice -= ParentServer_OnPrivateNotice;
            server.GotTopic -= ParentServer_OnGotTopic;
            server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            server.ErrorMessageRecieved -= ParentServer_OnError;
            server.Kick -= ParentServer_OnKick;
            server.Connecting -= Server_Connecting;
            server.Disconnected -= server_Disconnected;
        }

        private void server_Disconnected()
        {
            //intentionally blank
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(Server, e.Data));
            if (result != null && result.Result == CommandResult.Fail)
            {
                serverOutputBox.AppendError("\n" + result.Message);
            }
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            AddLine(ServerStrings.ConnectingMessage.With(server.URL, server.Port));
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
            AddLine(a.Code + " " + a.Message);
        }

        private void ParentServer_OnRawMessageReceived(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            //intentionally blank
            //AddLine(e.Data);
        }

        private void ParentServer_OnGotTopic(Channel chan, string topic)
        {
            chan.ShowTopic(topic);
        }

        private void ParentServer_OnPrivateNotice(object sender, UserMessageEventArgs e)
        {
            AddLine("-" + e.User.Nick + ":" + e.Message + "-");
        }

        private void Server_OnConnectFail(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            AddLine(ServerStrings.ConnectionFailedMessage.With(e.Data));
        }

        private void ParentServer_UserAction(object sender, ChannelMessageEventArgs e)
        {
            e.Channel.NewAction(e.User, e.Message);
        }

        private void ParentServer_OnRegistered()
        {
            // TODO: Join list of auto-join channels

            Invoke((MethodInvoker)delegate
            {
                // P90: Fix empty server description
                Text = String.Format("Status: {0} on {1} ({2}:{3})", server.UserNick,
                                     server.Description.Length == 0 ? server.URL : server.Description, server.URL,
                                     server.Port); //TODO: This should actually be the network name, not the server
            });
        }

        private void ParentServer_OnJoinSelf(object sender, OrtzIRC.Common.DataEventArgs<Channel> e)
        {
            ((MainForm)MdiParent).CreateChannelForm(e.Data);
        }

        private void ParentServer_ChannelMessaged(object sender, ChannelMessageEventArgs e)
        {
            e.Channel.NewMessage(e.User, e.Message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (server.IsConnected)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    DialogResult result = MessageBox.Show(ServerStrings.WarnDisconnect.With(Server.Description),
                                CommonStrings.DialogCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                    if (result == DialogResult.OK)
                    {
                        UnhookEvents();
                        server.Disconnect();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    UnhookEvents();
                    server.Disconnect();
                }
            }

            base.OnFormClosing(e);
        }

        private void AddLine(String text)
        {
            serverOutputBox.AppendLine(text);

            TextLoggerManager.TextEntry(Server, text + '\n');
        }
    }
}
