namespace OrtzIRC
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Resources;

    public partial class ServerForm : Form
    {
        private Server server;

        public ServerForm()
        {
            InitializeComponent();

            commandTextBox.Focus();
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
            server.JoinSelf += ParentServer_OnJoinSelf;
            server.ConnectFailed += Server_OnConnectFail;
            server.PrivateNotice += ParentServer_OnPrivateNotice;
            server.RawMessageReceived += ParentServer_OnRawMessageReceived;
            server.ErrorMessageRecieved += ParentServer_OnError;
            server.Connecting += Server_Connecting;
            server.Disconnected += server_Disconnected;
            server.ConnectionLost += server_ConnectionLost;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;
            serverOutputBox.MouseUp += serverOutputBox_MouseUp;
        }

        private void server_ConnectionLost(object sender, DataEventArgs<string> e)
        {
            AddLine("--Disconnected: " + e.Data); //hack - proper messages
        }

        private void serverOutputBox_MouseUp(object sender, MouseEventArgs e)
        {
            commandTextBox.Focus();
        }

        private void UnhookEvents()
        {
            server.Registered -= ParentServer_OnRegistered;
            server.JoinSelf -= ParentServer_OnJoinSelf;
            server.ConnectFailed -= Server_OnConnectFail;
            server.PrivateNotice -= ParentServer_OnPrivateNotice;
            server.RawMessageReceived -= ParentServer_OnRawMessageReceived;
            server.ErrorMessageRecieved -= ParentServer_OnError;
            server.Connecting -= Server_Connecting;
            server.Disconnected -= server_Disconnected;
            server.ConnectionLost -= server_ConnectionLost;
        }

        private void server_Disconnected()
        {
            AddLine("--Disconnected--");
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

        private void ParentServer_OnError(object sender, ErrorMessageEventArgs a)
        {
            AddLine(string.Format("{0} {1}", a.Code, a.Message));
        }

        private void ParentServer_OnRawMessageReceived(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            //intentionally blank
            //AddLine(e.Data);
        }

        private void ParentServer_OnPrivateNotice(object sender, UserMessageEventArgs e)
        {
            AddLine(string.Format("-{0}: {1}-", e.User.Nick, e.Message));
        }

        private void Server_OnConnectFail(object sender, OrtzIRC.Common.DataEventArgs<string> e)
        {
            AddLine(ServerStrings.ConnectionFailedMessage.With(e.Data));
        }

        private void ParentServer_OnRegistered(object sender, EventArgs e)
        {
            // TODO: Join list of auto-join channels
            if (InvokeRequired)
                Invoke(new Action(SetFormTitle));
            else
                SetFormTitle();
        }

        private void SetFormTitle()
        {
            Text = ServerStrings.ServerFormTitleBar.With(
                        server.UserNick,
                        server.Description == String.Empty ? server.URL : server.Description, server.URL,
                        server.Port
                        ); //TODO: This should actually be the network name, not the server
        }

        private void ParentServer_OnJoinSelf(object sender, OrtzIRC.Common.DataEventArgs<Channel> e)
        {
            ((MainForm)MdiParent).CreateChannelForm(e.Data);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ServerForm Closing: " + e.CloseReason);

            if (!server.IsConnected) return;
            if (e.CloseReason == CloseReason.MdiFormClosing) return;

            if (e.CloseReason != CloseReason.TaskManagerClosing)
            {
                DialogResult result = MessageBox.Show(ServerStrings.WarnDisconnect.With(Server.Description),
                    CommonStrings.DialogCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
            }

            UnhookEvents();
            server.Disconnect();
        }

        private void AddLine(String text)
        {
            serverOutputBox.AppendLine(text);

            TextLoggerManager.TextEntry(Server, text + '\n');
        }
    }
}
