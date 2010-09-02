using System.Threading;

namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using FlamingIRC;

    public delegate void Server_TopicRequestEventHandler(Channel chan, string topic);
    //public delegate void Server_NickEventHandler(User nick, string newNick);

    public sealed class Server : MessageContext
    {
        private DateTime serverChangeTime;

        public Server(ConnectionArgs settings)
        {
            SetupConnection(settings);
            PMSessions = new List<PrivateMessageSession>();
            HookEvents();
        }

        public string Url
        {
            get
            {
                return Connection.ConnectionData.Hostname;
            }
        }

        public int Port
        {
            get
            {
                return Connection.ConnectionData.Port;
            }
        }

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

        private void SetupConnection(ConnectionArgs args)
        {
            if (args.Nick == null)
                throw new ArgumentException("The ServerSettings.Nick property is null");

            ChanManager = new ChannelManager(this);

            Connection = new Connection(args, true, false);
            Connection.HandleNickTaken = false;
        }

        private void HookEvents()
        {
            Connection.ConnectionEstablished += Connection_OnConnectSuccess;
            Connection.ConnectFailed += Connection_ConnectFailed;
            Connection.ConnectionLost += Connection_ConnectionLost;

            Connection.Listener.OnJoin += Listener_OnJoin;
            Connection.Listener.OnPart += Listener_OnPart;
            Connection.Listener.OnPublic += Listener_OnPublic;
            Connection.Listener.OnRegistered += Listener_OnRegistered;
            Connection.Listener.OnNames += Listener_OnNames;
            Connection.Listener.OnChannelModeChange += Listener_OnChannelModeChange;
            Connection.Listener.OnUserModeChange += Listener_OnUserModeChange;
            Connection.Listener.OnError += Listener_OnError;
            Connection.Listener.OnAction += Listener_OnAction;
            Connection.Listener.OnPrivateNotice += Listener_OnPrivateNotice;
            Connection.Listener.OnRecieveTopic += ListenerOnRecieveTopic;
            Connection.Listener.OnNick += Listener_OnNick;
            Connection.Listener.OnKick += Listener_OnKick;
            Connection.Listener.OnPrivate += Listener_OnPrivate;
            Connection.Listener.OnPing += Listener_OnPing;
            Connection.Listener.OnNickError += Listener_OnNickError;

            Connection.RawMessageReceived += Connection_OnRawMessageReceived;
        }

        private void Listener_OnNickError(object sender, NickErrorEventArgs e)
        {
            NickError.Fire(this, e);
        }

        private void Listener_OnPing(string message)
        {
            PingReceived.Fire(this, new DataEventArgs<string>(message));
        }

        private void Connection_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            if (e.Reason == DisconnectReason.UserInitiated)
            {
                Disconnected.Fire(this, e);
            }
            else
            {
                ConnectionLost.Fire(this, e);
            }
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
            Connection.Listener.OnAction -= Listener_OnAction;
            Connection.Listener.OnPrivateNotice -= Listener_OnPrivateNotice;
            Connection.Listener.OnRecieveTopic -= ListenerOnRecieveTopic;
            Connection.Listener.OnNick -= Listener_OnNick;
            Connection.Listener.OnKick -= Listener_OnKick;
            Connection.Listener.OnPrivate -= Listener_OnPrivate;
            Connection.Listener.OnPing -= Listener_OnPing;

            Connection.RawMessageReceived -= Connection_OnRawMessageReceived;
        }

        /// <summary>
        /// The client joined a channel.
        /// </summary>
        public event EventHandler<DataEventArgs<Channel>> JoinSelf;
        public event EventHandler<CancelEventArgs> Connecting;

        /// <summary>
        /// Another user joined a channel.
        /// </summary>
        public event EventHandler<DoubleDataEventArgs<User, Channel>> JoinOther;

        /// <summary>
        /// A connect attempt failed.
        /// </summary>
        public event EventHandler<ConnectFailedEventArgs> ConnectFailed;

        public event EventHandler<DataEventArgs<string>> RawMessageReceived;
        public event EventHandler<ChannelMessageEventArgs> ChannelMessaged;
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<ErrorMessageEventArgs> ErrorMessageRecieved;
        public event EventHandler Registered;
        public event EventHandler<PartEventArgs> Part;
        public event EventHandler<PartEventArgs> PartSelf;
        public event EventHandler<ChannelModeChangeEventArgs> ChannelModeChange;
        public event UserModeChangeEventHandler UserModeChanged;
        public event EventHandler<CancelEventArgs> Disconnecting;
        public event EventHandler Disconnected;
        public event EventHandler<ChannelMessageEventArgs> UserAction;
        public event EventHandler<UserMessageEventArgs> PrivateNotice;
        public event Server_TopicRequestEventHandler GotTopic;
        public event EventHandler<NickChangeEventArgs> OnNick;
        public event NamesEventHandler OnNames;
        public event EventHandler<KickEventArgs> Kick;
        public event EventHandler<PrivateMessageSessionEventArgs> PrivateMessageSessionAdded;
        public event EventHandler<DisconnectEventArgs> ConnectionLost;
        public event EventHandler ConnectCancelled;
        public event EventHandler<DataEventArgs<string>> PingReceived;
        public event EventHandler<NickErrorEventArgs> NickError;

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
                ConnectCancelled.Fire(this, EventArgs.Empty);
                return;
            }

            if (DateTime.Now - serverChangeTime < TimeSpan.FromSeconds(1))
            {
                var th = new Thread((ThreadStart)delegate
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Connection.Connect();
                });
                th.Start();
            }
            else
            {
                Connection.Connect();
            }
        }

        public void Disconnect()
        {
            Disconnect("OrtzIRC (pre-alpha) - http://www.ortzirc.com/");
        }

        public void Disconnect(string reason)
        {
            Connection.Disconnect(reason);
            UnhookEvents();
        }

        private void Listener_OnKick(User user, string channel, string kickee, string reason)
        {
            var chan = ChanManager.GetChannel(channel);
            Kick.Fire(this, new KickEventArgs(user, chan, kickee, reason));
            chan.UserKick(user, kickee, reason);

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
            OnNick.Fire(this, new NickChangeEventArgs(user, newNick));
        }

        private void ListenerOnRecieveTopic(string channel, string topic)
        {
            if (GotTopic != null)
            {
                var chan = ChanManager.Create(channel);
                GotTopic(chan, topic);
                chan.ShowTopic(topic);
            }
        }

        private void Listener_OnPrivateNotice(User user, string notice)
        {
            PrivateNotice.Fire(this, new UserMessageEventArgs(user, notice));
        }

        private void Listener_OnAction(object sender, UserChannelMessageEventArgs ea)
        {
            var chan = ChanManager.Create(ea.Channel);
            UserAction.Fire(this, new ChannelMessageEventArgs(ea.User, chan, ea.Message));
            chan.OnNewAction(ea.User, ea.Message);
        }

        private void Connection_OnRawMessageReceived(object sender, FlamingDataEventArgs<string> e)
        {
            RawMessageReceived.Fire(this, new DataEventArgs<string>(e.Data));
        }

        private void Connection_OnConnectSuccess(object sender, EventArgs e)
        {
            Connected.Fire(this, EventArgs.Empty);
        }

        private void Connection_ConnectFailed(object sender, ConnectFailedEventArgs e)
        {
            ConnectFailed.Fire(this, e);
        }

        private void Listener_OnPublic(object sender, UserChannelMessageEventArgs ea)
        {
            ChannelMessaged.Fire(this, new ChannelMessageEventArgs(ea.User, ChanManager.GetChannel(ea.Channel), ea.Message));
            ChanManager.GetChannel(ea.Channel).OnNewMessage(ea.User, ea.Message);
        }

        private void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            if (OnNames != null)
                OnNames(channel, nicks, last);
        }

        private void Listener_OnJoin(User user, string channel)
        {
            var chan = ChanManager.Create(channel);
            if (user.Nick == UserNick)
            {
                JoinSelf.Fire(this, new DataEventArgs<Channel>(chan));

            }
            else
            {
                JoinOther.Fire(this, new DoubleDataEventArgs<User, Channel>(user, chan));
                chan.UserJoin(user);
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            var chan = ChanManager.GetChannel(channel);

            if (chan == null)
                return;

            if (IsMe(user))
            {
                PartSelf.Fire(this, new PartEventArgs(user, chan, String.Empty));
                return;
            }

            Part.Fire(this, new PartEventArgs(user, chan, reason));
            chan.UserPart(user, reason);

            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered(object sender, EventArgs e)
        {
            if (Registered != null)
                Registered(this, e);
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

        public Channel JoinChannel(string channelToJoin)
        {
            return JoinChannel(channelToJoin, String.Empty);
        }

        public Channel JoinChannel(string channelToJoin, string key)
        {
            Channel newChan = ChanManager.Create(channelToJoin);

            Connection.Sender.Join(channelToJoin, key); // TODO: Figure out what happens when you join with a wrong key, and fix up channel manager integrity afterwards.

            return newChan;
        }

        public void MessageUser(string nick, string msg)
        {
            Connection.Sender.PrivateMessage(nick, msg);
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", Url, Port);
        }

        public void ChangeNick(string nick)
        {
            Connection.Sender.Nick(nick);
        }

        public void ChangeServer(ConnectionArgs args)
        {
            if (IsConnected)
            {
                Disconnect();
            }

            if (args.Nick == null)
            {
                args.Nick = Connection.ConnectionData.Nick;
            }

            UnhookEvents();
            ChanManager.UnhookEvents();
            SetupConnection(args);
            HookEvents();

            serverChangeTime = DateTime.Now;
        }

        public void ChangeServer(string nick, string url, bool ssl)
        {
            ChangeServer(new ConnectionArgs(nick, url, ssl));
        }

        private bool IsMe(User user)
        {
            return user.Nick == Connection.ConnectionData.Nick;
        }
    }
}