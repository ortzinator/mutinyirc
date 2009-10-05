namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using FlamingIRC;

    public delegate void Server_TopicRequestEventHandler(Channel chan, string topic);
    public delegate void Server_NickEventHandler(User nick, string newNick);

    public class Server : MessageContext
    {
        public Server(ServerSettings settings) : this(settings.Url, settings.Description, 6667, settings.Ssl) //TODO: Select port from port list
        {
            //intentionally left blank
        }

        public Server(string uri, string description, int port, bool ssl)
        {
            URI = uri;
            Description = description;
            Port = port;
            SSL = ssl;
            
            ChanManager = new ChannelManager(this);

            var args = new ConnectionArgs("OrtzIRC", uri);
            Connection = new Connection(args, true, false);

            Connection.Listener.OnJoin += Listener_OnJoin;
            Connection.Listener.OnPart += Listener_OnPart;
            Connection.Listener.OnPublic += Listener_OnPublic;
            Connection.Listener.OnRegistered += Listener_OnRegistered;
            Connection.Listener.OnNames += Listener_OnNames;
            Connection.Listener.OnChannelModeChange += Listener_OnChannelModeChange;
            Connection.Listener.OnUserModeChange += Listener_OnUserModeChange;
            Connection.Listener.OnError += Listener_OnError;
            Connection.Listener.OnDisconnecting += Listener_OnDisconnecting;
            Connection.Listener.OnDisconnected += Listener_OnDisconnected;
            Connection.Listener.OnAction += Listener_OnAction;
            Connection.Listener.OnPrivateNotice += Listener_OnPrivateNotice;
            Connection.Listener.OnTopicRequest += Listener_OnTopicRequest;
            Connection.Listener.OnNick += Listener_OnNick;
            Connection.Listener.OnKick += Listener_OnKick;

            Connection.RawMessageReceived += Connection_OnRawMessageReceived;
            Connection.OnConnectSuccess += Connection_OnConnectSuccess;
            Connection.ConnectFailed += Connection_ConnectFailed;
        }

        public string URI { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public Connection Connection { get; private set; }

        public bool IsConnected
        {
            get { return Connection.Connected; }
        }

        /// <summary>
        /// The nick of the connected user
        /// </summary>
        public string UserNick
        {
            get { return Connection.ConnectionData.Nick; }
        }

        public ChannelManager ChanManager { get; private set; }

        public event EventHandler<DataEventArgs<Channel>> JoinSelf;
        public event EventHandler<CancelEventArgs> Connecting;
        public event EventHandler<DoubleDataEventArgs<User, Channel>> JoinOther;

        /// <summary>
        /// Informs the subscriber that a connect attempt fails.
        /// </summary>
        /// <remarks>
        /// The string data is the message about why the connection failed.
        /// </remarks>
        public event EventHandler<DataEventArgs<string>> ConnectFailed;

        public event EventHandler<DataEventArgs<string>> RawMessageReceived;
        public event EventHandler<ChannelMessageEventArgs> ChannelMessaged;
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<ErrorMessageEventArgs> ErrorMessageRecieved;
        public event RegisteredEventHandler Registered;
        public event EventHandler<PartEventArgs> Part;
        public event EventHandler<ChannelModeChangeEventArgs> ChannelModeChange;
        public event UserModeChangeEventHandler UserModeChanged;
        public event DisconnectingEventHandler Disconnecting;
        public event DisconnectedEventHandler Disconnected;
        public event EventHandler<ChannelMessageEventArgs> UserAction;
        public event EventHandler<UserMessageEventArgs> PrivateNotice;
        public event Server_TopicRequestEventHandler GotTopic;
        public event Server_NickEventHandler OnNick;
        public event NamesEventHandler OnNames;
        public event EventHandler<KickEventArgs> Kick;

        // hack - should call dispose
        ~Server()
        {
            if (Connection.Connected)
            {
                Disconnect();
            }

            ServerManager.Instance.Remove(this);
        }

        public void Connect()
        {
            DoConnect();
        }

        public void Disconnect()
        {
            Disconnect("OrtzIRC (pre-alpha) - http://www.ortzirc.com/"); //TODO: Pick random message from user-defined list of quit messages
        }

        public void Disconnect(string reason)
        {
            Connection.Disconnect(reason);
        }

        private void DoConnect()
        {
            CancelEventArgs c = new CancelEventArgs();

            Connecting.Fire(this, c);

            if (c.Cancel)
            {
                OnConnectFailed(new DataEventArgs<string>("Connect cancelled."));
                return;
            }

            try
            {
                Connection.Connect();
            }
            catch (Exception ex)
            {
                OnConnectFailed(new DataEventArgs<string>(ex.Message));
            }
        }

        private void Listener_OnKick(User user, string channel, string kickee, string reason)
        {
            OnKick(new KickEventArgs(user, ChanManager.GetChannel(channel), kickee, reason));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnUserModeChange(ModeAction action, UserMode mode)
        {
            if (UserModeChanged != null)
                UserModeChanged(action, mode);

            foreach (KeyValuePair<string, Channel> item in ChanManager.Channels)
                Connection.Sender.Names(item.Key);
        }

        private void Listener_OnNick(User user, string newNick)
        {
            if (OnNick != null)
                OnNick(user, newNick);
        }

        private void Listener_OnTopicRequest(string channel, string topic)
        {
            if (GotTopic != null)
                GotTopic(ChanManager.Create(channel), topic);
        }

        private void Listener_OnPrivateNotice(User user, string notice)
        {
            OnPrivateNotice(new UserMessageEventArgs(user, notice));
        }

        private void Listener_OnAction(User user, string channel, string description)
        {
            OnAction(new ChannelMessageEventArgs(user, ChanManager.Create(channel), description));
        }

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

        private void Connection_OnRawMessageReceived(object sender, FlamingDataEventArgs<string> e)
        {
            OnRawMessageReceived(new DataEventArgs<string>(e.Data));
        }

        private void Connection_OnConnectSuccess(object sender, EventArgs e)
        {
            OnConnected(EventArgs.Empty);
        }

        private void Connection_ConnectFailed(object sender, FlamingDataEventArgs<string> e)
        {
            OnConnectFailed(new DataEventArgs<string>(e.Data));
        }

        private void Listener_OnPublic(User user, string channel, string message)
        {
            OnPublicMessage(new ChannelMessageEventArgs(user, ChanManager.GetChannel(channel), message));
        }

        private void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            //ChanManager.OnNames(channel, nicks, last);
            if (OnNames != null)
                OnNames(channel, nicks, last);
        }

        private void Listener_OnJoin(User user, string channel)
        {
            if (user.Nick == UserNick)
            {
                OnJoinSelf(new DataEventArgs<Channel>(ChanManager.Create(channel)));
            }
            else
            {
                OnJoinOther(new DoubleDataEventArgs<User, Channel>(user, ChanManager.Create(channel)));
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            OnPart(new PartEventArgs(user, ChanManager.GetChannel(channel), reason));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered()
        {
            if (Registered != null)
                Registered();

            //TODO: Handle a taken nick
            //TODO: IIRC this isn't called if there's an error during registration. (The user's nick is taken)

            //TODO: Get autojoin list for the network
            JoinChannel(new ChannelInfo("#ortzirc"));
        }

        private void Listener_OnChannelModeChange(User who, string channel, ChannelModeInfo[] modes, string raw)
        {
            OnChannelModeChange(new ChannelModeChangeEventArgs(who, ChanManager.GetChannel(channel), modes, raw));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnError(object sender, ErrorMessageEventArgs a)
        {
            if (ErrorMessageRecieved != null)
                ErrorMessageRecieved(sender, new ErrorMessageEventArgs(a.Code, a.Message));
        }

        public Channel JoinChannel(ChannelInfo channelToJoin)
        {
            Channel newChan = ChanManager.Create(channelToJoin.ToString());

            Connection.Sender.Join(channelToJoin.ToString());

            return newChan;
        }

        protected virtual void OnPart(PartEventArgs e)
        {
            Part.Fire(this, e);
        }

        protected virtual void OnPublicMessage(ChannelMessageEventArgs e)
        {
            ChannelMessaged.Fire(this, e);
        }

        protected virtual void OnAction(ChannelMessageEventArgs e)
        {
            UserAction.Fire(this, e);
        }

        protected virtual void OnPrivateNotice(UserMessageEventArgs e)
        {
            PrivateNotice.Fire(this, e);
        }

        protected virtual void OnConnected(EventArgs e)
        {
            Connected.Fire(this, e);
        }

        protected virtual void OnKick(KickEventArgs e)
        {
            Kick.Fire(this, e);
        }

        protected virtual void OnRawMessageReceived(DataEventArgs<string> e)
        {
            RawMessageReceived(this, e);
        }

        protected virtual void OnConnecting(CancelEventArgs e)
        {
            Connecting.Fire(this, e);
        }

        protected virtual void OnJoinSelf(DataEventArgs<Channel> e)
        {
            JoinSelf.Fire(this, e);
        }

        protected virtual void OnJoinOther(DoubleDataEventArgs<User, Channel> e)
        {
            JoinOther.Fire(this, e);
        }

        protected virtual void OnConnectFailed(DataEventArgs<string> e)
        {
            ConnectFailed.Fire(this, e);
        }

        protected virtual void OnChannelModeChange(ChannelModeChangeEventArgs e)
        {
            ChannelModeChange.Fire(this, e);
        }

        public void MessageUser(string nick, string msg)
        {
            Connection.Sender.PrivateMessage(nick, msg);
        }

        public override string ToString()
        {
            return Description;
        }

        public void Nick(string nick)
        {
            Connection.Sender.Nick(nick);
        }
    }
}