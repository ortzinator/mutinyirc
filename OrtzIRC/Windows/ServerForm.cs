using System.Timers;

namespace OrtzIRC
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Resources;
    using OrtzIRC.Properties;

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
            server.ConnectCancelled += server_ConnectCancelled;
            server.NickError += server_NickError;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;
            serverOutputBox.MouseUp += serverOutputBox_MouseUp;
        }

        private int nickRetry;
        private bool nickRetryFailed;
        private void server_NickError(object sender, NickErrorEventArgs e)
        {
            if (server.Connection.Registered || server.Connection.HandleNickTaken) return;
            string newNick;
            switch (nickRetry)
            {
                case 0:
                    newNick = Settings.Default.SecondNick;
                    DisplayNickTakenMessage(e.BadNick, newNick);
                    server.Connection.Sender.Register(newNick);
                    nickRetry = 1;
                    break;
                case 1:
                    newNick = Settings.Default.ThirdNick;
                    DisplayNickTakenMessage(e.BadNick, newNick);
                    server.Connection.Sender.Register(Settings.Default.ThirdNick);
                    nickRetry = 2;
                    break;
            }

            if (nickRetry == 2 || nickRetryFailed)
            {
                nickRetryFailed = true;
                //If the following is successful then nickRetryFailed will be set back to false

                var generator = new NameGenerator();
                string nick;
                do
                {
                    nick = generator.MakeName();
                } while (!Rfc2812Util.IsValidNick(nick) || nick.Length == 1);
                //Try to reconnect
                server.Connection.Sender.Register(nick);
            }
        }

        private void DisplayNickTakenMessage(string nick, string newNick)
        {
            if (InvokeRequired)
                Invoke(new Action<string, string>(DisplayNickTakenMessage), nick, newNick);
            else
                AddLine(string.Format("The nick '{0}' was taken. Trying '{1}'", nick, newNick));
        }

        private void server_ConnectCancelled(object sender, EventArgs e)
        {
            AddLine("--Disconnected--");
        }

        private void server_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            if (e.SocketErrorCode == 0)
            {
                AddLine(string.Format("--Disconnected: {0}", e.Reason));
            }
            else
            {
                AddLine(string.Format("--Disconnected: {0} {1}", e.Reason, e.SocketErrorCode));
            }

            if (e.Reason != DisconnectReason.UserInitiated)
            {
                AddLine("Attempting to reconnect...");
                ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
            }
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
            server.ConnectCancelled -= server_ConnectCancelled;
        }

        private void server_Disconnected(object sender, EventArgs e)
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
            AddLine(ServerStrings.ConnectingMessage.With(server.Url, server.Port));
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

        private void Server_OnConnectFail(object sender, ConnectFailedEventArgs e)
        {
            if (e.SocketErrorCode == 0)
            {
                AddLine(ServerStrings.ConnectionFailedMessage.With(e.Reason.ToString()));
            }
            else
            {
                AddLine(ServerStrings.ConnectionFailedMessage.With(string.Format("{0}: {1}", e.Reason, e.SocketErrorCode)));
            }

            ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
        }

        private void ParentServer_OnRegistered(object sender, EventArgs e)
        {
            // TODO: Join list of auto-join channels
            if (InvokeRequired)
                Invoke(new Action(DoRegister));
            else
                DoRegister();
        }

        private void DoRegister()
        {
            Text = ServerStrings.ServerFormTitleBar.With(
                        server.UserNick,
                        server.Description == String.Empty ? server.Url : server.Description, server.Url,
                        server.Port
                        ); //TODO: This should actually be the network name, not the server


            if (nickRetryFailed)
            {
                AddLine("All of your alternate nicks were taken, so one was randomly chosen for you."); //TODO: Messagebox?
            }

            nickRetry = 0;
            nickRetryFailed = false;
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
