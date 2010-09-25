using System.ComponentModel;
using System.Diagnostics;
using OrtzIRC.WPF.Properties;

namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using OrtzIRC.Common;
    using System.Windows.Threading;
    using OrtzIRC.WPF.Resources;

    public class ServerViewModel : IrcViewModel
    {
        private int nickRetryAttempt;
        private bool nickRetryFailed;
        private Server server;

        public ServerViewModel(Server newServer)
        {
            System.Windows.DependencyObject dep = new System.Windows.DependencyObject();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(dep))
                return;

            server = newServer;
            Name = server.Url;
            //server.RawMessageReceived += Server_RawMessageReceived;
            server.Registered += Server_Registered;
            server.ConnectFailed += Server_ConnectFailed;
            server.PrivateNotice += Server_PrivateNotice;
            server.ErrorMessageRecieved += Server_ErrorMessageRecieved;
            server.Connecting += Server_Connecting;
            server.Disconnected += Server_Disconnected;
            server.ConnectionLost += Server_ConnectionLost;
            server.ConnectCancelled += Server_ConnectCancelled;
            server.NickError += Server_NickError;
            server.PartSelf += Server_PartSelf;
        }

        public ServerViewModel()
        {
            System.Windows.DependencyObject dep = new System.Windows.DependencyObject();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(dep))
            {
                ChatLines.Add(new ChatItemViewModel(DateTime.Now, "Foo"));
                ChatLines.Add(new ChannelMessageViewModel(DateTime.Now, "Message", "Ortzinator"));
                ChatLines.Add(new ChannelActionViewModel(DateTime.Now, "puts a donk on it", "Ortzinator"));
                ChatLines.Add(new IrcErrorViewModel(DateTime.Now, "Something bad happened", "14859"));
                ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, "A notice for you", "aUser"));
                Name = "irc.foo.com";
                return;
            }
        }

        private void Server_PartSelf(object sender, PartEventArgs e)
        {
            NetworkSettings nwSettings = IrcSettingsManager.Instance.GetNetwork(server);

            ChannelSettings chan = nwSettings.GetChannel(e.Channel.Name);

            chan.AutoJoin = false;
        }

        private void Server_NickError(object sender, NickErrorEventArgs e)
        {
            if (server.Connection.Registered || server.Connection.HandleNickTaken) return;
            string newNick;
            switch (nickRetryAttempt)
            {
                case 0:
                    newNick = Settings.Default.SecondNick;
                    DisplayNickTakenMessage(e.BadNick, newNick);
                    server.Connection.Sender.Register(newNick);
                    nickRetryAttempt = 1;
                    break;
                case 1:
                    newNick = Settings.Default.ThirdNick;
                    DisplayNickTakenMessage(e.BadNick, newNick);
                    server.Connection.Sender.Register(Settings.Default.ThirdNick);
                    nickRetryAttempt = 2;
                    break;
            }

            if (nickRetryAttempt == 2 || nickRetryFailed)
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
            AddMessage(ServerStrings.NickTakenMessage.With(nick, newNick));
        }

        private void Server_ConnectCancelled(object sender, EventArgs e)
        {
            AddMessage(ServerStrings.Disconnected);
        }

        private void Server_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            AddMessage(ServerStrings.ConnectionLost.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));

            if (e.Reason != DisconnectReason.UserInitiated)
            {
                AddMessage(ServerStrings.AttemptingReconnect);
                ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
            }
        }

        private void Server_Disconnected(object sender, EventArgs e)
        {
            AddMessage(ServerStrings.Disconnected);
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            AddMessage(ServerStrings.ConnectingMessage.With(server.Url, server.Port));
        }

        private void Server_ErrorMessageRecieved(object sender, ErrorMessageEventArgs e)
        {
            if (e.Code == ReplyCode.ERR_NOMOTD)
            {
                Debug.WriteLine("Message ignored: " + e.Code);
                return;
            }

            ChatLines.Add(new IrcErrorViewModel(DateTime.Now, e.Message, e.Code.ToString()));
        }

        private void Server_PrivateNotice(object sender, UserMessageEventArgs e)
        {
            ChatLines.Add(new PrivateNoticeViewModel(DateTime.Now, e.Message, e.User.Nick));
        }

        private void Server_ConnectFailed(object sender, ConnectFailedEventArgs e)
        {
            AddMessage(ServerStrings.ConnectionFailedMessage.With(SocketErrorTranslator.GetMessage(e.SocketErrorCode)));
            ThreadHelper.InvokeAfter(TimeSpan.FromSeconds(4), delegate { server.Connect(); });
        }

        private void Server_Registered(object sender, EventArgs e)
        {
            DoRegister();
        }

        private void Server_RawMessageReceived(object sender, DataEventArgs<string> e)
        {
            AddMessage(e.Data);
        }

        private void AddMessage(string message)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, message));
        }

        private void DoRegister()
        {
            string network = server.Connection.ServerProperties["Network"];
            NetworkSettings networkSettings = IrcSettingsManager.Instance.GetNetwork(server);

            if (networkSettings == null)
            {
                NetworkSettings tempNet;
                if (network == String.Empty)
                {
                    tempNet = IrcSettingsManager.Instance.AddNetwork(server.Url);
                    //TODO: Get domain name to use as network name, or something
                    network = "Network";
                }
                else
                {
                    tempNet = IrcSettingsManager.Instance.AddNetwork(network);
                }

                tempNet.AddServer(new ServerSettings(server.Url, "Random", server.Port.ToString(),
                        server.Connection.ConnectionData.Ssl) { AutoConnect = true });
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

                ServerSettings nServer = networkSettings.GetServer(server.Url);
                if (nServer == null)
                {
                    networkSettings.AddServer(new ServerSettings(server.Url, "Random", server.Port.ToString(),
                        server.Connection.ConnectionData.Ssl) { AutoConnect = true });
                }
            }

            Name = ServerStrings.ServerFormTitleBar.With(
                    server.UserNick,
                    network,
                    server.Url,
                    server.Port);

            if (nickRetryFailed)
                AddMessage(ServerStrings.RandomNickMessage);

            nickRetryAttempt = 0;
            nickRetryFailed = false;

            if (networkSettings == null || networkSettings.Channels == null) return;
            foreach (ChannelSettings channel in networkSettings.Channels)
            {
                if (channel.AutoJoin)
                    server.JoinChannel(channel.Name);
            }
        }
    }
}
