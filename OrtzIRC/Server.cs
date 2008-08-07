using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;
using OrtzIRC;

namespace OrtzIRC
{
    public delegate void Server_SelfJoinEventHandler(Channel chan);
    public delegate void Server_OtherJoinEventHandler(Nick nick, Channel chan);
    public delegate void Server_PartEventHandler(Nick nick, Channel chan, string message);
    public delegate void Server_ConnectingEventHandler();
    public delegate void Server_ConnectFailedEventHandler(string message);
    public delegate void Server_PublicMessageEventHandler(Nick nick, Channel chan, string message);
    public delegate void Server_ChannelModeChangeEventHandler(Nick nick, Channel chan, ChannelModeInfo[] modes, string raw);

    public class Server
    {
        public string URI { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public Connection Connection { get; private set; }
        public string UserNick { get; private set; }
        public string Name { get; private set; }

        public ChannelManager ChanManager { get; private set; }

        private delegate void NewChannelCallback(string channel);

        public event Server_SelfJoinEventHandler OnJoinSelf;
        public event Server_ConnectingEventHandler OnConnecting;
        public event Server_OtherJoinEventHandler OnJoinOther;
        public event Server_ConnectFailedEventHandler OnConnectFail;
        public event RawMessageReceivedEventHandler OnRawMessageReceived;
        public event Server_PublicMessageEventHandler OnPublicMessage;
        public event ConnectEventHandler OnConnectSuccess;
        public event ErrorMessageEventHandler OnError;
        public event RegisteredEventHandler OnRegistered;
        public event Server_PartEventHandler OnPart;
        public event Server_ChannelModeChangeEventHandler OnChannelModeChange;
        public event DisconnectingEventHandler OnDisconnecting;
        public event DisconnectedEventHandler OnDisconnected;

        public Server(ServerSettings settings)
        {
            this.URI = settings.Uri;
            this.Description = settings.Description;
            this.Port = settings.Port;
            this.SSL = settings.Ssl;
            //network.AddServer(this);

            this.ChanManager = new ChannelManager(this);

            this.UserNick = "OrtzIRC";

            ConnectionArgs args = new ConnectionArgs(this.UserNick, settings.Uri);
            this.Connection = new Connection(args, false, false);

            Connection.Listener.OnJoin += new JoinEventHandler(Listener_OnJoin);
            Connection.Listener.OnPart += new PartEventHandler(Listener_OnPart);
            Connection.Listener.OnPublic += new PublicMessageEventHandler(Listener_OnPublic);
            Connection.Listener.OnRegistered += new RegisteredEventHandler(Listener_OnRegistered);
            Connection.Listener.OnNames += new NamesEventHandler(Listener_OnNames);
            Connection.Listener.OnChannelModeChange += new ChannelModeChangeEventHandler(Listener_OnChannelModeChange);
            Connection.Listener.OnError += new ErrorMessageEventHandler(Listener_OnError);
            Connection.Listener.OnDisconnecting += new DisconnectingEventHandler(Listener_OnDisconnecting);
            Connection.Listener.OnDisconnected += new DisconnectedEventHandler(Listener_OnDisconnected);

            Connection.OnRawMessageReceived += new RawMessageReceivedEventHandler(Connection_OnRawMessageReceived);
            Connection.OnConnectSuccess += new ConnectEventHandler(Connection_OnConnectSuccess);

            try
            {
                Connection.Connect();
            }
            catch (Exception e)
            {
                if (OnConnectFail != null)
                    OnConnectFail(e.Message);
            }
        }

        public Server(string uri, string description, int port, bool ssl) : this(new ServerSettings(uri, description, port, ssl)) { }

        void Listener_OnDisconnected()
        {
            if (OnDisconnected != null)
                OnDisconnected();
        }

        void Listener_OnDisconnecting()
        {
            if (OnDisconnecting != null)
                OnDisconnecting();
        }

        void Connection_OnRawMessageReceived(string message)
        {
            if (OnRawMessageReceived != null)
                OnRawMessageReceived(message);
        }

        void Connection_OnConnectSuccess()
        {
            if (OnConnectSuccess != null)
                OnConnectSuccess();
        }

        private void Listener_OnPublic(UserInfo user, string channel, string message)
        {
            if (OnPublicMessage != null)
                OnPublicMessage(Nick.FromUserInfo(user), ChanManager.GetChannel(channel), message);
        }

        private void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            ChanManager.OnNames(channel, nicks, last);
        }

        private void Listener_OnJoin(UserInfo user, string channel)
        {
            if (user.Nick == UserNick)
            {
                if (OnJoinSelf != null)
                    OnJoinSelf(ChanManager.GetChannel(channel));
            }
            else
            {
                if (OnJoinOther != null)
                    OnJoinOther(Nick.FromUserInfo(user), ChanManager.Create(channel));

                Connection.Sender.Names(channel);
            }
        }

        private void Listener_OnPart(UserInfo user, string channel, string reason)
        {
            if (OnPart != null)
                OnPart(Nick.FromUserInfo(user), ChanManager.GetChannel(channel), reason);
            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered()
        {
            if (OnRegistered != null)
                OnRegistered();
            //JoinChannel("#ortzirc");
        }

        private void Listener_OnChannelModeChange(UserInfo who, string channel, ChannelModeInfo[] modes, string raw)
        {
            if (OnChannelModeChange != null)
                OnChannelModeChange(Nick.FromUserInfo(who), ChanManager.GetChannel(channel), modes, raw);
            Connection.Sender.Names(channel);
        }

        private void Listener_OnError(ReplyCode code, string message)
        {
            if (OnError != null)
                OnError(code, message);
        }

        public Channel JoinChannel(string channel)
        {
            Channel newChan = ChanManager.Create(channel);
            Connection.Sender.Join(channel);
            return newChan;
        }
    }
}
