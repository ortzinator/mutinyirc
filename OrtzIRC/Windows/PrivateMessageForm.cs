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
            pmsession.Server.PrivateNotice += ParentServer_OnPrivateNotice;
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
            pmsession.Server.PrivateNotice -= ParentServer_OnPrivateNotice;
            pmsession.Server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            pmsession.Server.ErrorMessageRecieved -= ParentServer_OnError;
            pmsession.Server.Kick -= ParentServer_OnKick;
            pmsession.Server.Disconnected -= server_Disconnected;

            pmsession.MessageReceived -= pmsession_MessageReceived;
        }

        private void pmsession_MessageSent(object sender, DataEventArgs<string> e)
        {
            string nick = PMSession.Server.Connection.ConnectionData.Nick;

            AddLine(ChannelStrings.PublicMessage.With(nick, e.Data));
        }

        private void pmsession_MessageReceived(object sender, DataEventArgs<string> e)
        {
            AddLine(ChannelStrings.PublicMessage.With(pmsession.User.Nick, e.Data));
        }

        private void serverOutputBox_MouseUp(object sender, MouseEventArgs e)
        {
            commandTextBox.Focus();
        }

        private void server_Disconnected(object sender, EventArgs e)
        {
            AddLine(ServerStrings.Disconnected);
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(PMSession, e.Data));
            if (result != null && result.Result == CommandResult.Fail)
            {
                serverOutputBox.AppendError("\n" + CommonStrings.CommandErrorMessage.With(result.Message));
            }
        }

        private void ParentServer_OnKick(object sender, KickEventArgs e)
        {
            AddLine(ChannelStrings.Kick.With(e.Kickee + ":" + e.Channel, e.User.Nick, e.Reason));
        }

        private void ParentServer_OnPart(object sender, PartEventArgs e)
        {
            AddLine(ChannelStrings.PartWithReason.With(e.User.Nick + ":" + e.Channel, e.User.HostMask, String.Empty));
        }

        private void ParentServer_OnJoinOther(object sender, OrtzIRC.Common.DoubleDataEventArgs<User, Channel> e)
        {
            AddLine(ChannelStrings.Joined.With(e.First.Nick + ":" + e.Second, e.First.HostMask));
        }

        private void ParentServer_OnError(object sender, ErrorMessageEventArgs a)
        {
            AddLine(ServerStrings.ServerErrorMessage.With(a.Code, a.Message));
        }

        private void ParentServer_OnRawMessageReceived(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            //intentionally blank
            //AddLine(e.Data);
        }

        private void ParentServer_OnPrivateNotice(object sender, UserMessageEventArgs e)
        {
            AddLine(CommonStrings.PrivateNotice.With(e.User.Nick, e.Message));
        }

        private void ParentServer_UserAction(object sender, ChannelMessageEventArgs e)
        {
            AddLine(ChannelStrings.Action.With(e.User.Nick, e.Message));
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
