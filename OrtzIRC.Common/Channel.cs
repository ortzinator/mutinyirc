namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public delegate void ReceivedNamesEventHandler(UserList nickList);
    public delegate void ChannelKickEventHandler(User nick, string kickee, string reason);

    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public sealed class Channel : MessageContext
    {
        public Channel(Server parent, string name)
        {
            Server = parent;
            Name = name;

            NickList = new UserList();
        }

        public Server Server { get; private set; }
        public string Key { get; set; }
        public int Limit { get; set; }
        public string Name { get; private set; }

        /// <summary>
        /// A UserList of the users in the channel
        /// </summary>
        /// <remarks>
        /// At the moment, this is kept up to date by requesting a NAMES list for the channel 
        /// whenever someone joins, parts, quits, or their mode is changed (and thus their prefix symbol).
        /// </remarks>
        public UserList NickList { get; private set; }

        /// <summary>
        /// Returns true if the user is in the channel
        /// </summary>
        public bool Joined
        {
            get
            {
                return NickList.Count > 0;
            }
        }

        //TODO: Update these to EventHandlers

        /// <summary>
        /// A user messaged the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnMessage;
        /// <summary>
        /// A user sent a message to the channel as an action
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnAction;
        /// <summary>
        /// The channel's topic was received.
        /// </summary>
        public event EventHandler<DataEventArgs<string>> TopicReceived;
        /// <summary>
        /// A user joined the channel
        /// </summary>
        public event EventHandler<UserEventArgs> OnJoin;
        /// <summary>
        /// A user parted the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OtherUserParted;
        /// <summary>
        /// A user in the channel quit from the server
        /// </summary>
        public event EventHandler<UserMessageEventArgs> UserQuitted;
        /// <summary>
        /// The client parted the channel
        /// </summary>
        public event EventHandler UserParted;
        /// <summary>
        /// A user in the channel changed his nickname
        /// </summary>
        public event EventHandler<NickChangeEventArgs> NickChanged;
        /// <summary>
        /// A NAMES list was recieved for the channel.
        /// </summary>
        public event ReceivedNamesEventHandler OnReceivedNames;
        /// <summary>
        /// A user was kicked from the channel
        /// </summary>
        public event ChannelKickEventHandler OnKick;
        /// <summary>
        /// The client messaged the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> MessagedChannel;

        public void AddNick(User nick)
        {
            NickList.Add(nick);
        }

        public override string ToString()
        {
            return Name;
        }

        public void OnNewMessage(User nick, string message)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    OnMessage.Fire(this, new UserMessageEventArgs(n, message));
                    break;
                }
            }
        }

        public void OnNewAction(User nick, string message)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    OnAction.Fire(this, new UserMessageEventArgs(n, message));
                    break;
                }
            }
        }

        public void ShowTopic(string topic)
        {
            TopicReceived.Fire(this, new DataEventArgs<string>(topic));
        }

        public void UserJoin(User nick)
        {
            OnJoin.Fire(this, new UserEventArgs(nick));
        }

        public void Part(string message)
        {
            Server.Connection.Sender.Part(message, Name);
            NickList.Clear();
        }

        public void Part()
        {
            Part("Leaving");
        }

        public void UserPart(User user, string message)
        {
            if (user.Nick != Server.UserNick)
                OtherUserParted.Fire(this, new UserMessageEventArgs(user, message));
            else
                UserParted.Fire(this, new EventArgs());
        }

        public void UserQuit(User user, string message)
        {
            //Make sure the user is in the channel
            foreach (User n in NickList)
            {
                if (user.Nick != n.Nick) continue;
                UserQuitted.Fire(this, new UserMessageEventArgs(n, message));
            }
        }

        public void NickChange(User nick, string newNick)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    NickChanged.Fire(this, new NickChangeEventArgs(nick, newNick));
                    Server.Connection.Sender.Names(Name);
                }
            }
        }

        public bool HasUser(string nick)
        {
            return NickList.Contains(User.FromNames(nick));
        }

        public void UserKick(User nick, string kickee, string reason)
        {
            Server.Connection.Sender.Names(Name);

            if (OnKick != null)
                OnKick(nick, kickee, reason);
        }

        public void Say(string message)
        {
            Server.Connection.Sender.PublicMessage(Name, message);
            MessagedChannel.Fire(this, new UserMessageEventArgs(NickList.GetUser(Server.UserNick), message));
        }

        public void Act(string message)
        {
            Server.Connection.Sender.Action(Name, message);
            MessagedChannel.Fire(this, new UserMessageEventArgs(NickList.GetUser(Server.UserNick), message));
        }
    }
}
