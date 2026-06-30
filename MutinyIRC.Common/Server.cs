using FlamingIRC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace MutinyIRC.Common
{
    public class Server : MessageContext, IDisposable
    {
        private DateTime _serverChangeTime;
        private IConnection _connection;
        private bool _disposed;

        // Automatic-reconnect policy. After a failed attempt or a non-user-initiated drop, the
        // next reconnect is scheduled with exponential backoff: ReconnectBaseDelay doubled per
        // consecutive attempt, capped at ReconnectMaxDelay. The counter resets on a successful
        // registration; the timer is cancelled on a user-initiated disconnect or Dispose so it
        // never revives a connection the user has quit.
        //
        // The drop/fail/registered events arrive on FlamingIRC's socket-IO threads while
        // Disconnect/Dispose run on the UI thread, so every read or write of the reconnect state
        // below is serialized through _reconnectGate. The timer callback re-checks, under the
        // gate, that it is still the installed timer before reconnecting, so a cancelled or
        // superseded timer that has already begun firing is a no-op.
        private static readonly TimeSpan ReconnectBaseDelay = TimeSpan.FromSeconds(4);
        private static readonly TimeSpan ReconnectMaxDelay = TimeSpan.FromMinutes(5);
        private readonly object _reconnectGate = new object();
        private int _reconnectAttempt;
        private Timer _reconnectTimer;

        /// <summary>
        ///   Nicknames whose PRIVMSGs bypass the PM tab UI and surface in the server
        ///   window instead (NickServ, ChanServ, etc). Hosts seed this per connection
        ///   (so it can vary by IRC network); tests can mutate it directly. Empty by
        ///   default so pure-protocol consumers see all PRIVMSGs as conversations.
        ///   Lookups are case-insensitive.
        /// </summary>
        public HashSet<string> ServiceNicks { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Server() { }

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

        /// <summary>A <see cref="Server"/> is its own context's server.</summary>
        public override Server OwningServer => this;

        public string Url => Connection.ConnectionData.Hostname;

        public int Port => Connection.ConnectionData.Port;

        public virtual IConnection Connection
        {
            get => _connection;
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                Channels.Clear();
                _connection = value;
            }
        }

        public List<PrivateMessageSession> PMSessions { get; } = new List<PrivateMessageSession>();

        public bool IsConnected => Connection.Connected;

        /// <summary>
        ///   Whether the local user is currently marked away. Set only in response to the
        ///   server confirming the change (<see cref="Listener.OnNowAway"/> /
        ///   <see cref="Listener.OnUnAway"/>), so it reflects the server's view rather than
        ///   an optimistic local guess.
        /// </summary>
        public bool IsAway { get; private set; }

        /// <summary>
        ///   The nick of the connected user
        /// </summary>
        public virtual string UserNick => Connection.ConnectionData.Nick;

        public Dictionary<string, Channel> Channels { get; } =
            new Dictionary<string, Channel>(StringComparer.OrdinalIgnoreCase);

        public static event EventHandler<ChannelEventArgs> ChannelCreated;

        public static event EventHandler<ChannelEventArgs> ChannelRemoved;

        public void SetupConnection(ConnectionArgs args)
        {
            if (args.Nick == null)
            {
                throw new ArgumentNullException("ConnectionArgs.Nick");
            }

            var conn = new Connection(args, true, false) { HandleNickTaken = false };
            Connection = conn;
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
            Connection.Listener.OnPrivateAction += Listener_OnPrivateAction;
            Connection.Listener.OnPrivateNotice += Listener_OnPrivateNotice;
            Connection.Listener.OnPublicNotice += Listener_OnPublicNotice;
            Connection.Listener.OnRecieveTopic += ListenerOnRecieveTopic;
            Connection.Listener.OnNick += Listener_OnNick;
            Connection.Listener.OnKick += Listener_OnKick;
            Connection.Listener.OnPrivate += Listener_OnPrivate;
            Connection.Listener.OnPing += Listener_OnPing;
            Connection.Listener.OnNickError += Listener_OnNickError;
            Connection.Listener.OnQuit += Listener_OnQuit;
            Connection.Listener.OnWhois += Listener_OnWhois;
            Connection.Listener.OnAway += Listener_OnAway;
            Connection.Listener.OnNowAway += Listener_OnNowAway;
            Connection.Listener.OnUnAway += Listener_OnUnAway;

            Connection.RawMessageReceived += Connection_OnRawMessageReceived;
        }

        private void Listener_OnQuit(User user, string reason)
        {
            foreach (KeyValuePair<string, Channel> pair in Channels)
            {
                pair.Value.UserQuit(user, reason);
            }
        }

        private void Listener_OnNickError(object sender, NickErrorEventArgs e)
        {
            NickError.Fire(this, e);
        }

        private void Listener_OnWhois(WhoisInfo whoisInfo)
        {
            WhoisReceived.Fire(this, new DataEventArgs<WhoisInfo>(whoisInfo));
        }

        private void Listener_OnPing(string message)
        {
            PingReceived.Fire(this, new DataEventArgs<string>(message));
        }

        private void Listener_OnAway(object sender, AwayEventArgs e)
        {
            AwayReplyReceived.Fire(this, e);
        }

        private void Listener_OnNowAway(object sender, EventArgs e)
        {
            IsAway = true;
            WentAway.Fire(this, EventArgs.Empty);
        }

        private void Listener_OnUnAway(object sender, EventArgs e)
        {
            IsAway = false;
            CameBack.Fire(this, EventArgs.Empty);
        }

        private void Connection_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            // Membership is connection-scoped: once the link drops we are no longer in any
            // channel, even though the Channel objects survive (their panels stay open so a
            // reconnect can rejoin in place). Reset each so nothing treats a stale channel as
            // joined while disconnected, and so a rejoin transitions cleanly through Joining.
            ResetChannelMembership();

            if (e.Reason == DisconnectReason.UserInitiated)
            {
                Disconnected.Fire(this, e);
            }
            else
            {
                ConnectionLost.Fire(this, e);
                ScheduleReconnect();
            }
        }

        /// <summary>
        ///   Marks every tracked channel <see cref="ChannelMembership.NotJoined"/>. Called when
        ///   the connection drops; the channels themselves are kept so their panels persist.
        /// </summary>
        private void ResetChannelMembership()
        {
            foreach (Channel chan in Channels.Values)
                chan.Membership = ChannelMembership.NotJoined;
        }

        private void Listener_OnPrivate(object sender, UserMessageEventArgs e)
        {
            if (IsServiceNick(e.User.Nick))
            {
                ServiceMessageReceived.Fire(this, new UserMessageEventArgs(e.User, e.Message));
                return;
            }

            GetOrCreatePM(e.User).OnMessageReceived(new DataEventArgs<string>(e.Message));
        }

        private void Listener_OnPrivateAction(object sender, UserMessageEventArgs e)
        {
            if (IsServiceNick(e.User.Nick))
            {
                ServiceActionReceived.Fire(this, new UserMessageEventArgs(e.User, e.Message));
                return;
            }

            GetOrCreatePM(e.User).OnActionReceived(new DataEventArgs<string>(e.Message));
        }

        private bool IsServiceNick(string nick)
            => !string.IsNullOrEmpty(nick) && ServiceNicks.Contains(nick);

        /// <summary>
        ///   Returns the existing <see cref="PrivateMessageSession"/> for the given user
        ///   or creates a new one. Returns null if the nickname identifies a server-side
        ///   service (per <see cref="ServiceNicks"/>) since services do not warrant
        ///   a dedicated PM tab.
        /// </summary>
        public PrivateMessageSession GetOrCreatePM(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrEmpty(user.Nick))
                throw new ArgumentException("User nick cannot be null or empty.", nameof(user));

            if (IsServiceNick(user.Nick))
                return null;

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

        /// <summary>
        ///   Convenience overload that resolves a <see cref="PrivateMessageSession"/> by
        ///   nickname. Returns null for service nicks.
        /// </summary>
        public PrivateMessageSession GetOrCreatePM(string nick)
        {
            if (string.IsNullOrEmpty(nick))
                throw new ArgumentException("Nick cannot be null or empty.", nameof(nick));

            return GetOrCreatePM(new User { Nick = nick });
        }

        /// <summary>
        ///   Removes a private message session from this server. Closure is UI-driven:
        ///   the host tears down its tab and then calls this to drop the session, so no
        ///   event is fired here. Safe to call with a session that is not currently tracked.
        /// </summary>
        public void RemovePM(PrivateMessageSession session)
        {
            if (session == null) return;

            PMSessions.Remove(session);
        }

        public void UnhookEvents()
        {
            Connection.ConnectionEstablished -= Connection_OnConnectSuccess;
            Connection.ConnectFailed -= Connection_ConnectFailed;
            Connection.ConnectionLost -= Connection_ConnectionLost;

            Connection.Listener.OnJoin -= Listener_OnJoin;
            Connection.Listener.OnPart -= Listener_OnPart;
            Connection.Listener.OnPublic -= Listener_OnPublic;
            Connection.Listener.OnRegistered -= Listener_OnRegistered;
            Connection.Listener.OnNames -= Listener_OnNames;
            Connection.Listener.OnChannelModeChange -= Listener_OnChannelModeChange;
            Connection.Listener.OnUserModeChange -= Listener_OnUserModeChange;
            Connection.Listener.OnError -= Listener_OnError;
            Connection.Listener.OnAction -= Listener_OnAction;
            Connection.Listener.OnPrivateAction -= Listener_OnPrivateAction;
            Connection.Listener.OnPrivateNotice -= Listener_OnPrivateNotice;
            Connection.Listener.OnPublicNotice -= Listener_OnPublicNotice;
            Connection.Listener.OnRecieveTopic -= ListenerOnRecieveTopic;
            Connection.Listener.OnNick -= Listener_OnNick;
            Connection.Listener.OnKick -= Listener_OnKick;
            Connection.Listener.OnPrivate -= Listener_OnPrivate;
            Connection.Listener.OnPing -= Listener_OnPing;
            Connection.Listener.OnNickError -= Listener_OnNickError;
            Connection.Listener.OnQuit -= Listener_OnQuit;
            Connection.Listener.OnWhois -= Listener_OnWhois;
            Connection.Listener.OnAway -= Listener_OnAway;
            Connection.Listener.OnNowAway -= Listener_OnNowAway;
            Connection.Listener.OnUnAway -= Listener_OnUnAway;

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

        /// <summary>
        ///   Fired when an automatic reconnect has been scheduled (after a failed attempt or a
        ///   non-user-initiated drop). The argument is the delay until that attempt, which grows
        ///   with exponential backoff. The reconnect runs internally, so hosts should render a
        ///   notice rather than calling <see cref="Connect"/> themselves.
        /// </summary>
        public event EventHandler<DataEventArgs<TimeSpan>> Reconnecting;

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

        /// <summary>
        ///   Fired when a PRIVMSG arrives from a nickname that <see cref="ServiceNicks"/>
        ///   identifies as a server-side service. No PM session is created; the host
        ///   should surface the message in the server window.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> ServiceMessageReceived;

        /// <summary>
        ///   Same as <see cref="ServiceMessageReceived"/> but for an incoming CTCP ACTION
        ///   (e.g. <c>/me</c>) from a service nick.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> ServiceActionReceived;

        /// <summary>
        ///   Fired when we send a PRIVMSG to a service nick (no PM tab exists for one).
        ///   The host echoes this in the server window so the outgoing line sits alongside
        ///   the service's replies (<see cref="ServiceMessageReceived"/>). The event's
        ///   <c>User</c> is the target service; the host renders the line under our own nick.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> ServiceMessageSent;

        public event EventHandler<DisconnectEventArgs> ConnectionLost;

        public event EventHandler ConnectCancelled;

        public event EventHandler<DataEventArgs<string>> PingReceived;

        public event EventHandler<NickErrorEventArgs> NickError;

        public event EventHandler<DataEventArgs<WhoisInfo>> WhoisReceived;

        /// <summary>
        ///   The server replied (RPL_AWAY) that a user we messaged is away. The event args
        ///   carry that user's nick and their away message.
        /// </summary>
        public event EventHandler<AwayEventArgs> AwayReplyReceived;

        /// <summary>
        ///   The server confirmed the local user is now marked away. <see cref="IsAway"/>
        ///   is already updated when this fires.
        /// </summary>
        public event EventHandler WentAway;

        /// <summary>
        ///   The server confirmed the local user is no longer away. <see cref="IsAway"/>
        ///   is already updated when this fires.
        /// </summary>
        public event EventHandler CameBack;

        /// <summary>
        ///   Tears the server down deterministically: disconnects if still connected (which
        ///   also unhooks the FlamingIRC handlers), otherwise just unhooks them, then drops out
        ///   of <see cref="ServerManager"/> so the singleton's list no longer pins this instance
        ///   in memory. The owner (the UI on shutdown) is responsible for calling this. Safe to
        ///   call more than once.
        ///
        ///   No finalizer backstops this: a Server is pinned by the ServerManager singleton (and,
        ///   while connected, by its own Connection event subscriptions) until Dispose removes it,
        ///   so it is never GC-eligible before Dispose has already run. Server owns no unmanaged
        ///   resource of its own — the socket lives in <see cref="Connection"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            // Mark disposed and drop the pending reconnect together under the gate the timer
            // callback locks on, so an in-flight callback either observes _disposed or finds its
            // timer cleared — it can never revive a Server that is being torn down.
            lock (_reconnectGate)
            {
                _disposed = true;
                _reconnectTimer?.Dispose();
                _reconnectTimer = null;
            }

            // _connection is null for a Server built with the parameterless constructor that
            // never had a Connection assigned; guard against an NRE.
            if (_connection != null)
            {
                if (_connection.Connected)
                    Disconnect();
                else
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
            Disconnect("MutinyIRC");
        }

        public void Disconnect(string reason)
        {
            CancelReconnect();
            Connection.Disconnect(reason);
            UnhookEvents();
        }

        /// <summary>
        ///   Schedules the next automatic reconnect with exponential backoff and announces it via
        ///   <see cref="Reconnecting"/>. The delay is ReconnectBaseDelay * 2^attempt clamped to
        ///   ReconnectMaxDelay; the attempt counter stops climbing once the cap is reached so it
        ///   can never run away. Any pending reconnect is cancelled first so only one is in flight.
        /// </summary>
        private void ScheduleReconnect()
        {
            TimeSpan delay;
            lock (_reconnectGate)
            {
                // A teardown that has already started must not be re-armed.
                if (_disposed)
                    return;

                delay = TimeSpan.FromSeconds(Math.Min(
                    ReconnectBaseDelay.TotalSeconds * Math.Pow(2, _reconnectAttempt),
                    ReconnectMaxDelay.TotalSeconds));
                if (delay < ReconnectMaxDelay)
                    _reconnectAttempt++;

                _reconnectTimer?.Dispose();
                Timer timer = null;
                timer = new Timer(_ =>
                {
                    lock (_reconnectGate)
                    {
                        // Only the timer still installed as _reconnectTimer may reconnect. A
                        // user-initiated Disconnect or a Dispose nulls it under this same gate, so
                        // it always wins the race against a callback that has already begun firing.
                        if (_disposed || !ReferenceEquals(_reconnectTimer, timer))
                            return;
                        Connect();
                    }
                }, null, delay, Timeout.InfiniteTimeSpan);
                _reconnectTimer = timer;
            }

            Reconnecting.Fire(this, new DataEventArgs<TimeSpan>(delay));
        }

        /// <summary>
        ///   Cancels any pending automatic reconnect. Safe to call when none is scheduled.
        /// </summary>
        private void CancelReconnect()
        {
            lock (_reconnectGate)
            {
                _reconnectTimer?.Dispose();
                _reconnectTimer = null;
            }
        }

        private void Listener_OnKick(User user, string channel, string kickee, string reason)
        {
            if (!Channels.TryGetValue(channel, out Channel chan))
                return;
            if (kickee == UserNick)
                chan.Membership = ChannelMembership.NotJoined;
            Kick.Fire(this, new KickEventArgs(user, chan, kickee, reason));
            chan.UserKick(user, kickee, reason);
        }

        private void Listener_OnUserModeChange(object sender, UserModeChangeEventArgs e)
        {
            UserModeChanged?.Invoke(this, e);
        }

        private void Listener_OnNick(object sender, NickChangeEventArgs e)
        {
            OnNick.Fire(this, new NickChangeEventArgs(e.User, e.NewNick));
        }

        private void ListenerOnRecieveTopic(string channel, string topic)
        {
            if (!Channels.TryGetValue(channel, out Channel chan))
                return;
            chan.ShowTopic(topic);
        }

        private void Listener_OnPrivateNotice(object sender, UserMessageEventArgs e)
        {
            // Where a private notice is displayed (active window vs. server window) is a UI
            // concern that depends on which panel is focused, so it's decided in the UI layer.
            // Here we just surface the event.
            PrivateNotice.Fire(this, new UserMessageEventArgs(e.User, e.Message));
        }

        private void Listener_OnPublicNotice(object sender, UserChannelMessageEventArgs ea)
        {
            if (!Channels.TryGetValue(ea.Channel, out Channel chan))
                return;
            chan.OnNewNotice(ea.User, ea.Message);
        }

        private void Listener_OnAction(object sender, UserChannelMessageEventArgs ea)
        {
            if (!Channels.TryGetValue(ea.Channel, out Channel chan))
                return;
            UserAction.Fire(this, new ChannelMessageEventArgs(ea.User, chan, ea.Message));
            chan.OnNewAction(ea.User, ea.Message);
        }

        private void Connection_OnRawMessageReceived(object sender,
                                                     FlamingIRC.DataEventArgs<string> e)
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
            ScheduleReconnect();
        }

        private void Listener_OnPublic(object sender, UserChannelMessageEventArgs ea)
        {
            if (!Channels.TryGetValue(ea.Channel, out Channel chan))
                return;
            ChannelMessaged.Fire(this,
                new ChannelMessageEventArgs(ea.User, chan, ea.Message));
            chan.OnNewMessage(ea.User, ea.Message);
        }

        private void Listener_OnNames(object sender, NamesEventArgs e)
        {
            OnNames?.Invoke(this, new NamesEventArgs(e.Channel, e.Nicks, e.Last));

            if (!Channels.TryGetValue(e.Channel, out Channel chan))
                return;

            chan.AddPendingNames(e.Nicks);

            Trace.WriteLine("Added chunk of " + e.Nicks.Length + " names", "Names");

            if (e.Last)
                chan.CommitPendingNames();
        }

        private void Listener_OnJoin(User user, string channel)
        {
            var chan = CreateChannel(channel);
            if (chan == null)
                return;
            if (user.Nick == UserNick)
            {
                chan.Membership = ChannelMembership.Joined;
                JoinSelf.Fire(this, new DataEventArgs<Channel>(chan));
                Connection.Sender.Names(channel);
            }
            else
            {
                JoinOther.Fire(this, new DoubleDataEventArgs<User, Channel>(user, chan));
                chan.UserJoin(user);
            }
        }

        private void Listener_OnPart(User user, string channel, string reason)
        {
            if (!Channels.TryGetValue(channel, out Channel chan))
                return;

            if (IsMe(user))
            {
                chan.Membership = ChannelMembership.NotJoined;
                Channels.Remove(chan.Name);
                ChannelRemoved.Fire(this, new ChannelEventArgs(chan));
                PartSelf.Fire(this, new PartEventArgs(user, chan, string.Empty));
                return;
            }

            Part.Fire(this, new PartEventArgs(user, chan, reason));
            chan.UserPart(user, reason);
        }

        private void Listener_OnRegistered(object sender, EventArgs e)
        {
            // Back online: clear the backoff so the next drop retries from ReconnectBaseDelay.
            lock (_reconnectGate)
                _reconnectAttempt = 0;
            Registered?.Invoke(this, e);
        }

        private void Listener_OnChannelModeChange(User who, string channel, ChannelModeInfo[] modes,
                                                  string raw)
        {
            if (!Channels.TryGetValue(channel, out Channel chan))
                return;
            ChannelModeChange.Fire(this, new ChannelModeChangeEventArgs(who, chan, modes, raw));

            chan.ApplyModeChanges(modes);
        }

        private void Listener_OnError(object sender, ErrorMessageEventArgs a)
        {
            CleanupFailedJoin(a);
            ErrorMessageRecieved?.Invoke(sender, new ErrorMessageEventArgs(a.Code, a.Message));
        }

        /// <summary>
        ///   Removes a channel from <see cref="Channels"/> when the server rejects our JOIN.
        /// </summary>
        /// <remarks>
        ///   <see cref="JoinChannel(string, string)"/> speculatively adds the channel before
        ///   the server confirms, so a rejection (wrong key, banned, invite-only, etc.) would
        ///   otherwise leave a phantom entry stuck in the <see cref="ChannelMembership.Joining"/>
        ///   state. Only channels still in that state are pruned, so a stray error for a channel
        ///   we've already joined cannot evict it.
        /// </remarks>
        private void CleanupFailedJoin(ErrorMessageEventArgs a)
        {
            if (!IsJoinFailureCode(a.Code) || string.IsNullOrEmpty(a.Message))
                return;

            int space = a.Message.IndexOf(' ');
            string channelName = space < 0 ? a.Message : a.Message.Substring(0, space);

            if (!Channels.TryGetValue(channelName, out Channel chan) ||
                chan.Membership != ChannelMembership.Joining)
                return;

            chan.Membership = ChannelMembership.NotJoined;
            Channels.Remove(chan.Name);
            ChannelRemoved.Fire(this, new ChannelEventArgs(chan));
        }

        private static bool IsJoinFailureCode(ReplyCode code)
        {
            return code switch
            {
                ReplyCode.ERR_NOSUCHCHANNEL => true,
                ReplyCode.ERR_TOOMANYCHANNELS => true,
                ReplyCode.ERR_CHANNELISFULL => true,
                ReplyCode.ERR_INVITEONLYCHAN => true,
                ReplyCode.ERR_BANNEDFROMCHAN => true,
                ReplyCode.ERR_BADCHANNELKEY => true,
                ReplyCode.ERR_BADCHANMASK => true,
                ReplyCode.ERR_NOCHANMODES => true,
                _ => false
            };
        }

        public Channel JoinChannel(string channelToJoin)
        {
            return JoinChannel(channelToJoin, string.Empty);
        }

        public Channel JoinChannel(string channelToJoin, string key)
        {
            Channel newChan = CreateChannel(channelToJoin);
            if (newChan == null)
                return null;
            newChan.Membership = ChannelMembership.Joining;

            Connection.Sender.Join(channelToJoin, key);

            return newChan;
        }

        /// <summary>
        ///   Sends a PRIVMSG to a user. The Server facade owns the Connection/Sender
        ///   plumbing so callers (e.g. <see cref="PrivateMessageSession"/>) don't reach
        ///   through it directly.
        /// </summary>
        public void MessageUser(string nick, string msg)
        {
            Connection.Sender.PrivateMessage(nick, msg);
        }

        /// <summary>
        ///   Sends a CTCP ACTION (a <c>/me</c>) to a user. Action counterpart to
        ///   <see cref="MessageUser"/>.
        /// </summary>
        public void MessageUserAction(string nick, string action)
        {
            Connection.Sender.PrivateAction(nick, action);
        }

        /// <summary>
        ///   Sends a PRIVMSG to a service nick (one that has no PM tab) and fires
        ///   <see cref="ServiceMessageSent"/> so the host can echo it in the server window.
        ///   Use this instead of <see cref="MessageUser"/> when the target is a service.
        /// </summary>
        public void MessageService(string nick, string msg)
        {
            MessageUser(nick, msg);
            ServiceMessageSent.Fire(this, new UserMessageEventArgs(new User { Nick = nick }, msg));
        }

        /// <summary>
        ///   Sends a NOTICE to a user or channel (the /notice command). Unlike
        ///   <see cref="MessageUser"/>, no <see cref="PrivateMessageSession"/> (and therefore no
        ///   PM tab) is created. The echo of the outgoing line is the caller's responsibility:
        ///   the /notice command raises <see cref="MessageContext.NoticeSent"/> on the context it
        ///   was issued from so the line appears in that window.
        /// </summary>
        public void SendNotice(string target, string message)
        {
            if (Rfc2812Util.IsValidChannelName(target))
                Connection.Sender.PublicNotice(target, message);
            else
                Connection.Sender.PrivateNotice(target, message);
        }

        /// <summary>
        ///   Marks the local user away with <paramref name="message"/> (the AWAY command). The
        ///   server's confirmation flips <see cref="IsAway"/> and raises <see cref="WentAway"/>.
        /// </summary>
        public void SetAway(string message)
        {
            Connection.Sender.Away(message);
        }

        /// <summary>
        ///   Clears the local user's away status. The server's confirmation flips
        ///   <see cref="IsAway"/> and raises <see cref="CameBack"/>.
        /// </summary>
        public void ClearAway()
        {
            Connection.Sender.UnAway();
        }

        /// <summary>
        ///   Sends a raw <c>MODE</c> command for <paramref name="target"/> (a channel or nick).
        ///   When <paramref name="modeArgs"/> is empty the target's current modes are requested
        ///   instead of changed.
        /// </summary>
        public void SendMode(string target, string modeArgs)
        {
            string command = string.IsNullOrWhiteSpace(modeArgs)
                ? $"MODE {target}"
                : $"MODE {target} {modeArgs}";
            Connection.Sender.Raw(command);
        }

        /// <summary>
        ///   Sends a raw, unparsed line straight to the server (the /raw command). The caller is
        ///   responsible for formatting a valid IRC command; nothing is added or escaped.
        /// </summary>
        public void SendRaw(string command)
        {
            Connection.Sender.Raw(command);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Url, Port);
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

            args.Nick ??= Connection.ConnectionData.Nick;

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

        /// <summary>
        ///   Returns the tracked <see cref="Channel"/> for the given name, creating and
        ///   registering it if absent. Returns null if the name is not a valid channel name.
        /// </summary>
        /// <remarks>
        ///   Channel creation belongs to the join lifecycle only (<see cref="JoinChannel(string, string)"/>
        ///   and <see cref="Listener_OnJoin"/>). Content events for a channel we never joined
        ///   (messages, actions, topics, modes, kicks, parts) must look the channel up via
        ///   <see cref="Channels"/> rather than calling this, so a stray event can't conjure a
        ///   phantom channel.
        /// </remarks>
        public Channel CreateChannel(string channelName)
        {
            if (Channels.ContainsKey(channelName))
            {
                return Channels[channelName];
            }

            if (!Rfc2812Util.IsValidChannelName(channelName))
                return null;

            var newChan = new Channel(this, channelName);
            Channels.Add(channelName, newChan);
            ChannelCreated.Fire(this, new ChannelEventArgs(newChan));

            return newChan;
        }

        public bool InChannel(string channelName)
        {
            return Channels.TryGetValue(channelName, out Channel chan) && chan.Joined;
        }
    }
}
