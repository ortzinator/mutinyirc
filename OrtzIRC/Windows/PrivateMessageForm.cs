namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Resources;

    public partial class PrivateMessageForm : Form
    {
        private PrivateMessageSession pmsession;

        public PrivateMessageForm()
        {
            InitializeComponent();

            commandTextBox.Focus();
        }

        public PrivateMessageSession PMSession
        {
            get { return pmsession; }
            set
            {
                if (pmsession != null)
                {
                    UnhookEvents();
                }

                pmsession = value;
                HookupEvents();
                SetFormTitle();
            }
        }

        private void HookupEvents()
        {
            pmsession.Server.UserAction += ParentServer_UserAction;
            pmsession.Server.JoinOther += ParentServer_OnJoinOther;
            pmsession.Server.Part += ParentServer_OnPart;
            pmsession.Server.ConnectFailed += Server_OnConnectFail;
            pmsession.Server.PrivateNotice += ParentServer_OnPrivateNotice;
            pmsession.Server.GotTopic += ParentServer_OnGotTopic;
            pmsession.Server.RawMessageReceived += ParentServer_OnRawMessageReceived;
            pmsession.Server.ErrorMessageRecieved += ParentServer_OnError;
            pmsession.Server.Kick += ParentServer_OnKick;
            pmsession.Server.Disconnected += server_Disconnected;

            pmsession.MessageReceived += pmsession_MessageReceived;
            pmsession.MessageSent += pmsession_MessageSent;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;
            serverOutputBox.MouseUp += serverOutputBox_MouseUp;
        }

        private void UnhookEvents()
        {
            pmsession.Server.UserAction -= ParentServer_UserAction;
            pmsession.Server.JoinOther -= ParentServer_OnJoinOther;
            pmsession.Server.Part -= ParentServer_OnPart;
            pmsession.Server.ConnectFailed -= Server_OnConnectFail;
            pmsession.Server.PrivateNotice -= ParentServer_OnPrivateNotice;
            pmsession.Server.GotTopic -= ParentServer_OnGotTopic;
            pmsession.Server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            pmsession.Server.ErrorMessageRecieved -= ParentServer_OnError;
            pmsession.Server.Kick -= ParentServer_OnKick;
            pmsession.Server.Disconnected -= server_Disconnected;

            pmsession.MessageReceived -= pmsession_MessageReceived;
        }

        private void pmsession_MessageSent(object sender, DataEventArgs<string> e)
        {
            string nick = PMSession.Server.Connection.ConnectionData.Nick;

            AddLine(string.Format("{0}: {1}", nick, e.Data));
        }

        private void pmsession_MessageReceived(object sender, DataEventArgs<string> e)
        {
            AddLine(string.Format("{0}: {1}", pmsession.User.Nick, e.Data));
        }

        private void serverOutputBox_MouseUp(object sender, MouseEventArgs e)
        {
            commandTextBox.Focus();
        }
        private void server_Disconnected()
        {
            AddLine("--Disconnected--");
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(PMSession, e.Data));
            if (result != null && result.Result == CommandResult.Fail)
            {
                serverOutputBox.AppendError("\n" + result.Message);
            }
        }

        private void ParentServer_OnKick(object sender, KickEventArgs e)
        {
            //e.Channel.UserKick(e.User, e.Kickee, e.Reason);
        }

        private void ParentServer_OnPart(object sender, PartEventArgs e)
        {
            //e.Channel.UserPart(e.User, e.Reason);
        }

        private void ParentServer_OnJoinOther(object sender, OrtzIRC.Common.DoubleDataEventArgs<User, Channel> e)
        {
            //e.Second.UserJoin(e.First);
        }

        private void ParentServer_OnError(object sender, ErrorMessageEventArgs a)
        {
            AddLine(string.Format("{0} {1}", a.Code, a.Message));
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
            AddLine(string.Format("-{0}: {1}-", e.User.Nick, e.Message));
        }

        private void Server_OnConnectFail(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            AddLine(ServerStrings.ConnectionFailedMessage.With(e.Data));
        }

        private void ParentServer_UserAction(object sender, ChannelMessageEventArgs e)
        {
            //e.Channel.OnNewAction(e.User, e.Message);
        }

        private void SetFormTitle()
        {
            Text = pmsession.User.Nick;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookEvents();
        }

        private void AddLine(string text)
        {
            serverOutputBox.AppendLine(text);

            TextLoggerManager.TextEntry(pmsession.Server, pmsession.User, text + '\n');
        }
    }
}
