namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using FlamingIRC;

    public delegate void Server_TopicRequestEventHandler(Channel chan, string topic);
    public delegate void Server_NickEventHandler(User nick, string newNick);

    public sealed class Server : MessageContext
    {
        public Server(ServerSettings settings)
        {
            SetupConnection(settings);
            PMSessions = new List<PrivateMessageSession>();
            HookEvents();
        }

        private void SetupConnection(ServerSettings settings)
        {
            Settings = settings;
            ChanManager = new ChannelManager(this);

            //TODO: Select port
            //TODO: Select nick
            var args = new ConnectionArgs("OrtzIRC", settings.Url, settings.Ssl);
            Connection = new Connection(args, true, false);
        }

        private void HookEvents()
        {
            Connection.OnConnectSuccess += Connection_OnConnectSuccess;
            Connection.ConnectFailed += Connection_ConnectFailed;

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
            Connection.Listener.OnPrivate += Listener_OnPrivate;

            Connection.RawMessageReceived += Connection_OnRawMessageReceived;
        }

        private void Listener_OnPrivate(User user, string message)
        {
            GetPM(user).OnMessageReceived(new DataEventArgs<string>(message));
        }

        private PrivateMessageSession GetPM(User user)
        {
            foreach (var session in PMSessions)
            {
                if (session.User.Equals(user))
                {
                    return session;
                }
            }

            var tmpsession = new PrivateMessageSession(this, user);
            PMSessions.Add(tmpsession);
            PrivateMessageSessionAdded.Fire(this, new PrivateMessageSessionEventArgs(tmpsession));
            return tmpsession;
        }

        private void UnhookEvents()
        {
            Connection.Listener.OnJoin -= Listener_OnJoin;
            Connection.Listener.OnPart -= Listener_OnPart;
            Connection.Listener.OnPublic -= Listener_OnPublic;
            Connection.Listener.OnRegistered -= Listener_OnRegistered;
            Connection.Listener.OnNames -= Listener_OnNames;
            Connection.Listener.OnChannelModeChange -= Listener_OnChannelModeChange;
            Connection.Listener.OnUserModeChange -= Listener_OnUserModeChange;
            Connection.Listener.OnError -= Listener_OnError;
            Connection.Listener.OnDisconnecting -= Listener_OnDisconnecting;
            Connection.Listener.OnDisconnected -= Listener_OnDisconnected;
            Connection.Listener.OnAction -= Listener_OnAction;
            Connection.Listener.OnPrivateNotice -= Listener_OnPrivateNotice;
            Connection.Listener.OnTopicRequest -= Listener_OnTopicRequest;
            Connection.Listener.OnNick -= Listener_OnNick;
            Connection.Listener.OnKick -= Listener_OnKick;
            Connection.Listener.OnPrivate -= Listener_OnPrivate;

            Connection.RawMessageReceived -= Connection_OnRawMessageReceived;
        }

        public string URL
        {
            get
            {
                return Settings.Url;
            }
        }

        public string Description
        {
            get
            {
                return Settings.Description ?? String.Empty;
            }
        }

        public int Port
        {
            get
            {
                return Connection.ConnectionData.Port;
            }
        }

        public string Nick
        {
            get
            {
                return Connection.ConnectionData.Nick;
            }
        }

        public ServerSettings Settings { get; private set; }

        public Connection Connection { get; private set; }

        public List<PrivateMessageSession> PMSessions { get; private set; }

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
        public event EventHandler Registered;
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
        public event EventHandler<PrivateMessageSessionEventArgs> PrivateMessageSessionAdded;

        // hack - should call dispose
        ~Server()
        {
            if (Connection.Connected)
            {
                Disconnect();
            }
            else
            {
                UnhookEvents();
            }

            ServerManager.Instance.Remove(this);
        }

        public void Connect()
        {
            var c = new CancelEventArgs();

            Connecting.Fire(this, c);

            if (c.Cancel)
            {
                ConnectFailed.Fire(this, new DataEventArgs<string>("Connect cancelled."));
                return;
            }

            try
            {
                Connection.Connect();
            }
            catch (Exception ex)
            {
                ConnectFailed.Fire(this, new DataEventArgs<string>(ex.Message));
            }
        }

        public void Disconnect()
        {
            Disconnect("OrtzIRC (pre-alpha) - http://www.ortzirc.com/"); //TODO: Pick random message from user-defined list of quit messages
        }

        public void Disconnect(string reason)
        {
            Connection.Disconnect(reason);
            UnhookEvents();
        }

        private void Listener_OnKick(User user, string channel, string kickee, string reason)
        {
            Kick.Fire(this, new KickEventArgs(user, ChanManager.GetChannel(channel), kickee, reason));

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
            PrivateNotice.Fire(this, new UserMessageEventArgs(user, notice));
        }

        private void Listener_OnAction(User user, string channel, string description)
        {
            UserAction.Fire(this, new ChannelMessageEventArgs(user, ChanManager.Create(channel), description));
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
            RawMessageReceived.Fire(this, new DataEventArgs<string>(e.Data));
        }

        private void Connection_OnConnectSuccess(object sender, EventArgs e)
        {
            Connected.Fire(this, EventArgs.Empty);
        }

        private void Connection_ConnectFailed(object sender, FlamingDataEventArgs<string> e)
        {
            ConnectFailed.Fire(this, new DataEventArgs<string>(e.Data));
        }

        private void Listener_OnPublic(User user, string channel, string message)
        {
            ChannelMessaged.Fire(this, new ChannelMessageEventArgs(user, ChanManager.GetChannel(channel), message));
            ChanManager.GetChannel(channel).OnNewMessage(user, message);
        }

        private void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            if (OnNames != null)
                OnNames(channel, nicks, last);
        }

        private void Listener_OnJoin(User user, string channel)
        {
            if (user.Nick == UserNick)
            {
                JoinSelf.Fire(this, new DataEventArgs<Channel>(ChanManager.Create(channel)));
            }
            else
            {
                JoinOther.Fire(this, new DoubleDataEventArgs<User, Channel>(user, ChanManager.Create(channel)));
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            Part.Fire(this, new PartEventArgs(user, ChanManager.GetChannel(channel), reason));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered(object sender, EventArgs e)
        {
            if (Registered != null)
                Registered(this, e);

            // TODO: Handle a taken nick
            //TODO: Get autojoin list for the network
            //HACK: This is for testing. It just gets a list and joins it all. Should be done by the GUI layer
            if (Settings.Channels != null)
            {
                foreach (var channel in Settings.Channels)
                    JoinChannel(new ChannelInfo(channel));
            }

            //JoinChannel(new ChannelInfo("#ortzirc"));
        }

        private void Listener_OnChannelModeChange(User who, string channel, ChannelModeInfo[] modes, string raw)
        {
            ChannelModeChange.Fire(this, new ChannelModeChangeEventArgs(who, ChanManager.GetChannel(channel), modes, raw));

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

        public void MessageUser(string nick, string msg)
        {
            Connection.Sender.PrivateMessage(nick, msg);
        }

        public override string ToString()
        {
            return Description;
        }

        public void ChangeNick(string nick)
        {
            Connection.Sender.Nick(nick);
        }

        public void ChangeServer(ServerSettings settings)
        {
            if (IsConnected)
            {
                Disconnect();
            }

            UnhookEvents();
            ChanManager.UnhookEvents();
            SetupConnection(settings);
            HookEvents();
        }
    }
}