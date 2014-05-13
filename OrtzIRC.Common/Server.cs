using FlamingIRC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace OrtzIRC.Common
{
    public class Server : MessageContext
    {
        private Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();
        private List<PrivateMessageSession> _pmSessions = new List<PrivateMessageSession>();
        private bool _recievingNames;
        private DateTime _serverChangeTime;
        private List<User> _tempNames = new List<User>();
        private Connection _connection;

        public Server(ConnectionArgs settings)
        {
            SetupConnection(settings);
            HookEvents();
        }

        public Server(Connection connection)
        {
            Connection = connection;
            HookEvents();
        }

        public string Url
        {
            get { return Connection.ConnectionData.Hostname; }
        }

        public int Port
        {
            get { return Connection.ConnectionData.Port; }
        }

        public Connection Connection
        {
            get { return _connection; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Channels.Clear();
                _connection = value;
            }
        }

        public List<PrivateMessageSession> PMSessions
        {
            get { return _pmSessions; }
        }

        public bool IsConnected
        {
            get { return Connection.Connected; }
        }

        /// <summary>
        ///   The nick of the connected user
        /// </summary>
        public string UserNick
        {
            get { return Connection.ConnectionData.Nick; }
        }

        public Dictionary<string, Channel> Channels
        {
            get { return _channels; }
        }

        public static event EventHandler<ChannelEventArgs> ChannelCreated;

        public static event EventHandler<ChannelEventArgs> ChannelRemoved;

        public void SetupConnection(ConnectionArgs args)
        {
            if (args.Nick == null)
            {
                throw new ArgumentNullException("ConnectionArgs.Nick");
            }

            Connection = new Connection(args, true, false) { HandleNickTaken = false };
        }

        public void HookEvents()
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
            Connection.Listener.OnQuit += Listener_OnQuit;

            Connection.RawMessageReceived += Connection_OnRawMessageReceived;
        }

        private void Listener_OnQuit(User user, string reason)
        {
            foreach (KeyValuePair<string, Channel> pair in _channels)
            {
                pair.Value.UserQuit(user, reason);
            }
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

        private void Listener_OnPrivate(object sender, UserMessageEventArgs e)
        {
            GetPM(e.User).OnMessageReceived(new DataEventArgs<string>(e.Message));
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

        public void UnhookEvents()
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
        ///   The client joined a channel.
        /// </summary>
        public event EventHandler<DataEventArgs<Channel>> JoinSelf;

        public event EventHandler<CancelEventArgs> Connecting;

        /// <summary>
        ///   Another user joined a channel.
        /// </summary>
        public event EventHandler<DoubleDataEventArgs<User, Channel>> JoinOther;

        /// <summary>
        ///   A connect attempt failed.
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

        public event EventHandler<UserModeChangeEventArgs> UserModeChanged;

        public event EventHandler Disconnected;

        public event EventHandler<ChannelMessageEventArgs> UserAction;

        public event EventHandler<UserMessageEventArgs> PrivateNotice;

        public event EventHandler<NickChangeEventArgs> OnNick;

        public event EventHandler<NamesEventArgs> OnNames;

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

            //Don't attept to connect more often than once a second
            if (DateTime.Now - _serverChangeTime < TimeSpan.FromSeconds(1))
            {
                var th = new Thread(() =>
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
            var chan = _channels[channel];
            Kick.Fire(this, new KickEventArgs(user, chan, kickee, reason));
            chan.UserKick(user, kickee, reason);

            Connection.Sender.Names(channel);
        }

        private void Listener_OnUserModeChange(object sender, UserModeChangeEventArgs e)
        {
            if (UserModeChanged != null)
                UserModeChanged(this, e);

            foreach (KeyValuePair<string, Channel> item in _channels)
                Connection.Sender.Names(item.Key);
        }

        private void Listener_OnNick(object sender, NickChangeEventArgs e)
        {
            OnNick.Fire(this, new NickChangeEventArgs(e.User, e.NewNick));

            foreach (KeyValuePair<string, Channel> item in _channels)
            {
                if (item.Value.HasUser(e.User.Nick))
                {
                    item.Value.NickChange(e.User, e.NewNick);
                }
            }
        }

        private void ListenerOnRecieveTopic(string channel, string topic)
        {
            var chan = CreateChannel(channel);
            chan.ShowTopic(topic);
        }

        private void Listener_OnPrivateNotice(object sender, UserMessageEventArgs e)
        {
            PrivateNotice.Fire(this, new UserMessageEventArgs(e.User, e.Message));
        }

        private void Listener_OnAction(object sender, UserChannelMessageEventArgs ea)
        {
            var chan = CreateChannel(ea.Channel);
            UserAction.Fire(this, new ChannelMessageEventArgs(ea.User, chan, ea.Message));
            chan.OnNewAction(ea.User, ea.Message);
        }

        private void Connection_OnRawMessageReceived(object sender, FlamingIRC.DataEventArgs<string> e)
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
            ChannelMessaged.Fire(this, new ChannelMessageEventArgs(ea.User, _channels[ea.Channel], ea.Message));
            _channels[ea.Channel].OnNewMessage(ea.User, ea.Message);
        }

        private void Listener_OnNames(object sender, NamesEventArgs e)
        {
            if (OnNames != null)
                OnNames(this, new NamesEventArgs(e.Channel, e.Nicks, e.Last));

            Channel chan = _channels[e.Channel];
            if (!_recievingNames)
            {
                _recievingNames = true;
            }

            foreach (string nick in e.Nicks)
            {
                _tempNames.Add(User.FromNames(nick));
            }

            Trace.WriteLine("Added chunk of " + e.Nicks.Length + " names", "Names");

            if (e.Last)
            {
                chan.LoadNewNames(_tempNames);
                _recievingNames = false;
                _tempNames.Clear();
            }
        }

        private void Listener_OnJoin(User user, string channel)
        {
            var chan = CreateChannel(channel);
            if (user.Nick == UserNick)
            {
                JoinSelf.Fire(this, new DataEventArgs<Channel>(chan));
            }
            else
            {
                JoinOther.Fire(this, new DoubleDataEventArgs<User, Channel>(user, chan));
                chan.UserJoin(user);
                Connection.Sender.Names(channel);
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            var chan = _channels[channel];

            if (chan == null)
                return;

            if (IsMe(user))
            {
                PartSelf.Fire(this, new PartEventArgs(user, chan, String.Empty));
                return;
            }

            Part.Fire(this, new PartEventArgs(user, chan, reason));
            chan.UserPart(user, reason);

            _channels.Remove(chan.Name);
            ChannelRemoved.Fire(this, new ChannelEventArgs(chan));

            Connection.Sender.Names(channel);
        }

        private void Listener_OnRegistered(object sender, EventArgs e)
        {
            if (Registered != null)
                Registered(this, e);
        }

        private void Listener_OnChannelModeChange(User who, string channel, ChannelModeInfo[] modes, string raw)
        {
            ChannelModeChange.Fire(this, new ChannelModeChangeEventArgs(who, _channels[channel], modes, raw));

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
            Channel newChan = CreateChannel(channelToJoin);

            Connection.Sender.Join(channelToJoin, key);
            // TODO: Figure out what happens when you join with a wrong key, and fix up channel manager integrity afterwards.

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
            SetupConnection(args);
            HookEvents();

            _serverChangeTime = DateTime.Now;
        }

        public void ChangeServer(string nick, string url, bool ssl)
        {
            ChangeServer(new ConnectionArgs(nick, url, ssl));
        }

        private bool IsMe(User user)
        {
            return user.Nick == Connection.ConnectionData.Nick;
        }

        public Channel CreateChannel(string channelName)
        {
            if (_channels.ContainsKey(channelName))
            {
                return _channels[channelName];
            }

            if (!Rfc2812Util.IsValidChannelName(channelName))
                return null;

            var newChan = new Channel(this, channelName);
            _channels.Add(channelName, newChan);
            ChannelCreated.Fire(this, new ChannelEventArgs(newChan));

            return newChan;
        }

        public bool InChannel(string channelName)
        {
            if (_channels.ContainsKey(channelName))
            {
                if (_channels[channelName].Joined)
                {
                    return true;
                }

                ChannelRemoved.Fire(this, new ChannelEventArgs(_channels[channelName]));
                if (!_channels.Remove(channelName))
                    Debug.WriteLine("Failed to remove channel");
                //TODO: Is this all that needs to be done?
            }
            return false;
        }
    }
}