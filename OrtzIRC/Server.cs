namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using OrtzIRC.Common;
    using Sharkbite.Irc;

    public delegate void Server_ConnectFailedEventHandler(string message);
    public delegate void Server_ChannelModeChangeEventHandler(Nick nick, Channel chan, ChannelModeInfo[] modes, string raw);
    public delegate void Server_ChannelMessageEventHandler(Nick nick, Channel chan, string message);
    public delegate void Server_PrivateNoticeEventHandler(Nick nick, string message);
    public delegate void Server_TopicRequestEventHandler(Channel chan, string topic);
    public delegate void Server_NickEventHandler(Nick nick, string newNick);
    public delegate void Server_KickEventHandler(Nick nick, Channel chan, string kickee, string reason);


    public class Server
    {
        public string URI { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public Connection Connection { get; private set; }
        public string UserNick { get; private set; }

        public ChannelManager ChanManager { get; private set; }

        public event EventHandler<DataEventArgs<Channel>> JoinSelf;
        public event EventHandler<EventArgs> Connecting;
        public event EventHandler<DoubleDataEventArgs<Nick, Channel>> JoinOther;
        public event Server_ConnectFailedEventHandler ConnectFail;
        public event RawMessageReceivedEventHandler RawMessageReceived;
        public event Server_ChannelMessageEventHandler PublicMessage;
        public event ConnectEventHandler ConnectSuccess;
        public event ErrorMessageEventHandler Error;
        public event RegisteredEventHandler Registered;
        public event Server_ChannelMessageEventHandler Part;
        public event Server_ChannelModeChangeEventHandler ChannelModeChange;
        public event UserModeChangeEventHandler UserModeChange;
        public event DisconnectingEventHandler Disconnecting;
        public event DisconnectedEventHandler Disconnected;
        public event Server_ChannelMessageEventHandler Action;
        public event Server_PrivateNoticeEventHandler PrivateNotice;
        public event Server_TopicRequestEventHandler GotTopic;
        public event Server_NickEventHandler OnNick;
        public event NamesEventHandler OnNames;
        public event Server_KickEventHandler Kick;

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
            this.Connection = new Connection(args, true, false);

            Connection.Listener.OnJoin += new JoinEventHandler(Listener_OnJoin);
            Connection.Listener.OnPart += new PartEventHandler(Listener_OnPart);
            Connection.Listener.OnPublic += new PublicMessageEventHandler(Listener_OnPublic);
            Connection.Listener.OnRegistered += new RegisteredEventHandler(Listener_OnRegistered);
            Connection.Listener.OnNames += new NamesEventHandler(Listener_OnNames);
            Connection.Listener.OnChannelModeChange += new ChannelModeChangeEventHandler(Listener_OnChannelModeChange);
            Connection.Listener.OnUserModeChange += new UserModeChangeEventHandler(Listener_OnUserModeChange);
            Connection.Listener.OnError += new ErrorMessageEventHandler(Listener_OnError);
            Connection.Listener.OnDisconnecting += new DisconnectingEventHandler(Listener_OnDisconnecting);
            Connection.Listener.OnDisconnected += new DisconnectedEventHandler(Listener_OnDisconnected);
            Connection.Listener.OnAction += new ActionEventHandler(Listener_OnAction);
            Connection.Listener.OnPrivateNotice += new PrivateNoticeEventHandler(Listener_OnPrivateNotice);
            Connection.Listener.OnTopicRequest += new TopicRequestEventHandler(Listener_OnTopicRequest);
            Connection.Listener.OnNick += new NickEventHandler(Listener_OnNick);
            Connection.Listener.OnKick += new KickEventHandler(Listener_OnKick);

            Connection.OnRawMessageReceived += new RawMessageReceivedEventHandler(Connection_OnRawMessageReceived);
            Connection.OnConnectSuccess += new ConnectEventHandler(Connection_OnConnectSuccess);

            try
            {
                Connection.Connect();
            }
            catch (Exception e)
            {
                if (ConnectFail != null)
                    ConnectFail(e.Message);
            }
        }

        private void Listener_OnKick(UserInfo user, string channel, string kickee, string reason)
        {
            if (Kick != null)
                Kick(Nick.FromUserInfo(user), ChanManager.GetChannel(channel), kickee, reason);
            Connection.Sender.Names(channel);
        }

        private void Listener_OnUserModeChange(ModeAction action, UserMode mode)
        {
            if (UserModeChange != null)
                UserModeChange(action, mode);
            foreach (KeyValuePair<string, Channel> item in ChanManager.Channels)
            {
                Connection.Sender.Names(item.Key);
            }
            
        }

        private void Listener_OnNick(UserInfo user, string newNick)
        {
            if (OnNick != null)
                OnNick(Nick.FromUserInfo(user), newNick);
        }

        private void Listener_OnTopicRequest(string channel, string topic)
        {
            if (GotTopic != null)
                GotTopic(ChanManager.Create(channel), topic);
        }

        private void Listener_OnPrivateNotice(UserInfo user, string notice)
        {
            if (PrivateNotice != null)
                PrivateNotice(Nick.FromUserInfo(user), notice);
        }

        private void Listener_OnAction(UserInfo user, string channel, string description)
        {
            if (Action != null)
                Action(Nick.FromUserInfo(user), ChanManager.Create(channel), description);
        }

        public Server(string uri, string description, int port, bool ssl) : this(new ServerSettings(uri, description, port, ssl)) { }

        private void Listener_OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }

        private void Listener_OnDisconnecting()
        {
            if (Disconnecting != null)
                Disconnecting();
        }

        private void Connection_OnRawMessageReceived(string message)
        {
            if (RawMessageReceived != null)
                RawMessageReceived(message);
        }

        private void Connection_OnConnectSuccess()
        {
            if (ConnectSuccess != null)
                ConnectSuccess();
        }

        private void Listener_OnPublic(UserInfo user, string channel, string message)
        {
            if (PublicMessage != null)
                PublicMessage(Nick.FromUserInfo(user), ChanManager.GetChannel(channel), message);
        }

        private void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            //ChanManager.OnNames(channel, nicks, last);
            if (OnNames != null)
                OnNames(channel, nicks, last);
        }

        private void Listener_OnJoin(UserInfo user, string channel)
        {
            if (user.Nick == UserNick)
            {
                this.OnJoinSelf(new DataEventArgs<Channel>(ChanManager.Create(channel)));
            }
            else
            {
                this.OnJoinOther(new DoubleDataEventArgs<Nick, Channel>(Nick.FromUserInfo(user), ChanManager.Create(channel)));
            }
        }

        private void Listener_OnPart(UserInfo user, string channel, string reason)
        {
            if (Part != null)
                Part(Nick.FromUserInfo(user), ChanManager.GetChannel(channel), reason);
            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered()
        {
            if (Registered != null)
                Registered();
            //JoinChannel("#ortzirc");
        }

        private void Listener_OnChannelModeChange(UserInfo who, string channel, ChannelModeInfo[] modes, string raw)
        {
            if (ChannelModeChange != null)
                ChannelModeChange(Nick.FromUserInfo(who), ChanManager.GetChannel(channel), modes, raw);
            Connection.Sender.Names(channel);
        }

        private void Listener_OnError(ReplyCode code, string message)
        {
            if (Error != null)
                Error(code, message);
        }

        public Channel JoinChannel(string channel)
        {
            Channel newChan = ChanManager.Create(channel);
            Connection.Sender.Join(channel);
            return newChan;
        }

        protected virtual void OnJoinSelf(DataEventArgs<Channel> e)
        {
            JoinSelf.Fire<DataEventArgs<Channel>>(this, e);
        }

        protected virtual void OnJoinOther(DoubleDataEventArgs<Nick, Channel> e)
        {
            JoinOther.Fire<DoubleDataEventArgs<Nick, Channel>>(this, e);
        }
    }
}