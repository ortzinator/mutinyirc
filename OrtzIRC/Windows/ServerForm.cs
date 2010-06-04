namespace OrtzIRC
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;
    using OrtzIRC.Properties;
    using OrtzIRC.Resources;
    using System.Reflection;

    public partial class ServerForm : Form
    {
        private int nickRetry;
        private bool nickRetryFailed;
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
                server.Connection.Sender.Register(nick);
            }
        }

        private void DisplayNickTakenMessage(string nick, string newNick)
        {
            if (InvokeRequired)
                Invoke(new Action<string, string>(DisplayNickTakenMessage), nick, newNick);
            else
                AddLine(ServerStrings.NickTakenMessage.With(nick, newNick));
        }

        private void server_ConnectCancelled(object sender, EventArgs e)
        {
            AddLine(ServerStrings.Disconnected);
        }

        private void server_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            AddLine(ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));

            if (e.Reason != DisconnectReason.UserInitiated)
            {
                AddLine(ServerStrings.AttemptingReconnect);
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
            AddLine(ServerStrings.Disconnected);
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            CommandResultInfo result = new CommandResultInfo();
            try
            {
                result = PluginManager.ExecuteCommand(PluginManager.ParseCommand(Server, e.Data));
            }
            catch (TargetInvocationException ex)
            {
                serverOutputBox.AppendError("\n" + ex.InnerException.Message);
            }

            if (result != null && result.Result == CommandResult.Fail)
                serverOutputBox.AppendError("\n" + result.Message);
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            AddLine(ServerStrings.ConnectingMessage.With(server.Url, server.Port));
        }

        private void ParentServer_OnError(object sender, ErrorMessageEventArgs a)
        {
            if (a.Code == ReplyCode.ERR_NOMOTD)
            {
                Debug.WriteLine("Message ignored: " + a.Code);
                return;
            }

            AddLine(string.Format("{0} {1}", a.Code, a.Message));
        }

        private void ParentServer_OnRawMessageReceived(object sender, DataEventArgs<string> e)
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
            AddLine(ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));

            ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
        }

        private void ParentServer_OnRegistered(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action(DoRegister));
            else
                DoRegister();
        }

        private void DoRegister()
        {
            string network = Server.Connection.ServerProperties["Network"];
            NetworkSettings networkSettings = IrcSettingsManager.Instance.GetNetwork(Server);

            if (networkSettings == null)
            {
                NetworkSettings tempNet;
                if (network == String.Empty)
                {
                    tempNet = IrcSettingsManager.Instance.AddNetwork(Server.Url); //TODO: Get domain name or something
                    network = "Network"; //HACK
                }
                else
                {
                    tempNet = IrcSettingsManager.Instance.AddNetwork(network);

                }

                tempNet.AddServer(new ServerSettings(Server.Url, "Random", Server.Port.ToString(),
                        Server.Connection.ConnectionData.Ssl) { AutoConnect = true });
            }
            else
            {
                if (network == String.Empty)
                {
                    network = networkSettings.Name;
                }
                else
                {
                    networkSettings.Name = network;
                }

                ServerSettings nServer = networkSettings.GetServer(Server.Url);
                if (nServer == null)
                {
                    networkSettings.AddServer(new ServerSettings(Server.Url, "Random", Server.Port.ToString(),
                        Server.Connection.ConnectionData.Ssl) { AutoConnect = true });
                }
            }

            Text = ServerStrings.ServerFormTitleBar.With(
                    server.UserNick,
                    network,
                    server.Url,
                    server.Port);

            if (nickRetryFailed)
                AddLine(ServerStrings.RandomNickMessage); //TODO: Messagebox?

            nickRetry = 0;
            nickRetryFailed = false;

            if (networkSettings != null)
            {
                if (networkSettings.Channels != null)
                {
                    foreach (ChannelSettings channel in networkSettings.Channels)
                    {
                        if (channel.AutoJoin)
                            Server.JoinChannel(channel.Name);
                    }
                }
            }
        }

        private void ParentServer_OnJoinSelf(object sender, DataEventArgs<Channel> e)
        {
            ((MainForm)MdiParent).CreateChannelForm(e.Data);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.MdiFormClosing) return;
            if (e.CloseReason != CloseReason.TaskManagerClosing)
            {
                if (server.IsConnected)
                {
                    DialogResult result = MessageBox.Show(ServerStrings.WarnDisconnect.With(Server.Url),
                                                          CommonStrings.DialogCaption, MessageBoxButtons.OKCancel,
                                                          MessageBoxIcon.Warning);

                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else
                {
                    IrcSettingsManager.Instance.DisableAutoConnect(Server);
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