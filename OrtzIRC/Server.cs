namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using FlamingIRC;
    using OrtzIRC.Common;

    public delegate void Server_TopicRequestEventHandler(Channel chan, string topic);
    public delegate void Server_NickEventHandler(User nick, string newNick);

    public class Server
    {
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
        public event EventHandler<ChannelMessageEventArgs> PublicMessage;
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<ErrorMessageEventArgs> ErrorMessageRecieved;
        public event RegisteredEventHandler Registered;
        public event EventHandler<PartEventArgs> Part;
        public event EventHandler<ChannelModeChangeEventArgs> ChannelModeChange;
        public event UserModeChangeEventHandler UserModeChanged;
        public event DisconnectingEventHandler Disconnecting;
        public event DisconnectedEventHandler Disconnected;
        public event EventHandler<ChannelMessageEventArgs> UserAction;
        public event EventHandler<PrivateMessageEventArgs> PrivateNotice;
        public event Server_TopicRequestEventHandler GotTopic;
        public event Server_NickEventHandler OnNick;
        public event NamesEventHandler OnNames;
        public event EventHandler<KickEventArgs> Kick;

        public string URI { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public Connection Connection { get; private set; }
        public bool IsConnected { get { return Connection.Connected; } }
        public string UserNick { get; private set; }

        public ChannelManager ChanManager { get; private set; }

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
            Connection.Listener.OnError += new EventHandler<ErrorMessageEventArgs>(Listener_OnError);
            Connection.Listener.OnDisconnecting += new DisconnectingEventHandler(Listener_OnDisconnecting);
            Connection.Listener.OnDisconnected += new DisconnectedEventHandler(Listener_OnDisconnected);
            Connection.Listener.OnAction += new ActionEventHandler(Listener_OnAction);
            Connection.Listener.OnPrivateNotice += new PrivateNoticeEventHandler(Listener_OnPrivateNotice);
            Connection.Listener.OnTopicRequest += new TopicRequestEventHandler(Listener_OnTopicRequest);
            Connection.Listener.OnNick += new NickEventHandler(Listener_OnNick);
            Connection.Listener.OnKick += new KickEventHandler(Listener_OnKick);

            Connection.RawMessageReceived += new EventHandler<FlamingDataEventArgs<string>>(Connection_OnRawMessageReceived);
            //Connection.OnConnectSuccess += new ConnectEventHandler(Connection_OnConnectSuccess);
            Connection.ConnectFailed += new EventHandler<FlamingDataEventArgs<string>>(Connection_ConnectFailed);

            DoConnect();
        }

        public Server(string uri, string description, int port, bool ssl)
            : this(new ServerSettings(uri, description, port, ssl))
        {
            //intentionally left blank
        }

        // hack - should call dispose
        ~Server()
        {
            this.Disconnect();
        }

        public void Connect()
        {
            DoConnect();
        }

        public void Disconnect()
        {
            this.Connection.Disconnect("User exited...");
        }

        public void Disconnect(string reason)
        {
            this.Connection.Disconnect(reason);
        }

        // hack - needs to try connection in the background
        private void DoConnect()
        {
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
            this.OnKick(new KickEventArgs(user, ChanManager.GetChannel(channel), kickee, reason));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnUserModeChange(ModeAction action, UserMode mode)
        {
            if (UserModeChanged != null)
                UserModeChanged(action, mode);
            foreach (KeyValuePair<string, Channel> item in ChanManager.Channels)
            {
                Connection.Sender.Names(item.Key);
            }

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
            this.OnPrivateNotice(new PrivateMessageEventArgs(user, notice));
        }

        private void Listener_OnAction(User user, string channel, string description)
        {
            this.OnAction(new ChannelMessageEventArgs(user, ChanManager.Create(channel), description));
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
            this.OnRawMessageReceived(new DataEventArgs<string>(e.Data));
        }

        private void Connection_OnConnectSuccess(object sender, EventArgs e)
        {
            this.OnConnected(EventArgs.Empty);
        }

        private void Connection_ConnectFailed(object sender, FlamingDataEventArgs<string> e)
        {
            this.OnConnectFailed(new DataEventArgs<string>(e.Data));
        }

        private void Listener_OnPublic(User user, string channel, string message)
        {
            this.OnPublicMessage(new ChannelMessageEventArgs(user, ChanManager.GetChannel(channel), message));
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
                this.OnJoinSelf(new DataEventArgs<Channel>(ChanManager.Create(channel)));
            }
            else
            {
                this.OnJoinOther(new DoubleDataEventArgs<User, Channel>(user, ChanManager.Create(channel)));
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            this.OnPart(new PartEventArgs(user, ChanManager.GetChannel(channel), reason));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered()
        {
            if (Registered != null)
                Registered();
            JoinChannel("#ortzirc");
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

        public Channel JoinChannel(string channel)
        {
            Channel newChan = ChanManager.Create(channel);
            Connection.Sender.Join(channel);
            return newChan;
        }

        protected virtual void OnPart(PartEventArgs e)
        {
            this.Part.Fire<PartEventArgs>(this, e);
        }

        protected virtual void OnPublicMessage(ChannelMessageEventArgs e)
        {
            this.PublicMessage.Fire<ChannelMessageEventArgs>(this, e);
        }

        protected virtual void OnAction(ChannelMessageEventArgs e)
        {
            this.UserAction.Fire<ChannelMessageEventArgs>(this, e);
        }

        protected virtual void OnPrivateNotice(PrivateMessageEventArgs e)
        {
            this.PrivateNotice.Fire<PrivateMessageEventArgs>(this, e);
        }

        protected virtual void OnConnected(EventArgs e)
        {
            this.Connected.Fire<EventArgs>(this, e);
        }

        protected virtual void OnKick(KickEventArgs e)
        {
            this.Kick.Fire<KickEventArgs>(this, e);
        }

        protected virtual void OnRawMessageReceived(DataEventArgs<string> e)
        {
            this.RawMessageReceived(this, e);
        }

        protected virtual void OnConnecting(CancelEventArgs e)
        {
            this.Connecting.Fire<CancelEventArgs>(this, e);
        }

        protected virtual void OnJoinSelf(DataEventArgs<Channel> e)
        {
            JoinSelf.Fire<DataEventArgs<Channel>>(this, e);
        }

        protected virtual void OnJoinOther(DoubleDataEventArgs<User, Channel> e)
        {
            JoinOther.Fire<DoubleDataEventArgs<User, Channel>>(this, e);
        }

        protected virtual void OnConnectFailed(DataEventArgs<string> e)
        {
            ConnectFailed.Fire<DataEventArgs<string>>(this, e);
        }

        protected virtual void OnChannelModeChange(ChannelModeChangeEventArgs e)
        {
            ChannelModeChange.Fire<ChannelModeChangeEventArgs>(this, e);
        }
    }
}
