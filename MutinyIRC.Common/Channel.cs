using System;
using System.Collections.Generic;
using FlamingIRC;

namespace MutinyIRC.Common
{
    public delegate void ChannelKickEventHandler(User nick, string kickee, string reason);

    /// <summary>
    ///   Our membership state for a channel, independent of whether the user list is populated.
    /// </summary>
    public enum ChannelMembership
    {
        /// <summary>We are not a member of the channel.</summary>
        NotJoined,

        /// <summary>A JOIN has been sent but the server has not yet echoed it back.</summary>
        Joining,

        /// <summary>The server has confirmed we are a member of the channel.</summary>
        Joined
    }

    /// <summary>
    ///   Represents a specific channel on a network
    /// </summary>
    public sealed class Channel : MessageContext
    {
        public Channel(Server parent, string name)
        {
            Server = parent;
            Name = name;
            Users = new UserList();
            Server.OnNick += Server_OnNick;
        }

        /// <summary>
        ///   The Server object the channel is associated with
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        ///   The key (password) to the channel
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///   The user limit of the channel
        /// </summary>
        /// <remarks>
        ///   For informational purposes only
        /// </remarks>
        public int Limit { get; set; }

        /// <summary>
        ///   The name of the channel, including any prefix symbols.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///   A UserList of the users in the channel
        /// </summary>
        /// <remarks>
        ///   Populated from the NAMES reply on channel join, then maintained incrementally
        ///   as users join, part, quit, are kicked, or have their op/voice mode changed.
        /// </remarks>
        public UserList Users { get; set; }

        /// <summary>
        ///   Our membership state for this channel. Set from the connection lifecycle
        ///   (join sent, join echoed, part, kick), not derived from the user list.
        /// </summary>
        public ChannelMembership Membership { get; internal set; } = ChannelMembership.NotJoined;

        /// <summary>
        ///   Returns true if the server has confirmed we are a member of the channel.
        /// </summary>
        public bool Joined => Membership == ChannelMembership.Joined;

        //TODO: Update these to EventHandlers

        /// <summary>
        ///   A user messaged the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnMessage;

        /// <summary>
        ///   A user sent a message to the channel as an action
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnAction;

        /// <summary>
        ///   The channel's topic was received.
        /// </summary>
        public event EventHandler<DataEventArgs<string>> TopicReceived;

        /// <summary>
        ///   A user joined the channel
        /// </summary>
        public event EventHandler<UserEventArgs> OnJoin;

        /// <summary>
        ///   A user parted the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OtherUserParted;

        /// <summary>
        ///   A user in the channel quit from the server
        /// </summary>
        public event EventHandler<UserMessageEventArgs> UserQuitted;

        /// <summary>
        ///   The client parted the channel
        /// </summary>
        public event EventHandler UserParted;

        /// <summary>
        ///   A user in the channel changed his nickname
        /// </summary>
        public event EventHandler<NickChangeEventArgs> NickChanged;

        /// <summary>
        ///   A NAMES list was recieved for the channel.
        /// </summary>
        public event EventHandler<DataEventArgs<UserList>> OnReceivedNames;

        /// <summary>
        ///   A user was kicked from the channel
        /// </summary>
        public event ChannelKickEventHandler OnKick;

        /// <summary>
        ///   The client messaged the channel
        /// </summary>
        public event EventHandler<UserMessageEventArgs> MessagedChannel;

        public void Server_OnNick(object sender, NickChangeEventArgs e)
        {
            User user = Users.GetUser(e.User);
            if (user != null) { user.Nick = e.NewNick; }
        }

        /// <summary>
        /// Adds a user to the user list.
        /// </summary>
        /// <param name="nick">The user to add</param>
        public void AddNick(User nick)
        {
            Users.Add(nick);
        }

        public override string ToString()
        {
            return Name;
        }

        public void OnNewMessage(User nick, string message)
        {
            foreach (User n in Users)
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
            foreach (User n in Users)
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
            if (!Users.Contains(nick))
                Users.Add(nick);
            OnJoin.Fire(this, new UserEventArgs(nick));
        }

        /// <summary>
        ///   Removes the user with the given nick from the channel's user list, if present.
        /// </summary>
        public void RemoveUser(string nick)
        {
            User u = Users.GetUser(nick);
            if (u != null)
                Users.Remove(u);
        }

        /// <summary>
        ///   Applies channel op/halfop/voice mode changes to the affected users' prefixes
        ///   without re-fetching the whole NAMES list.
        /// </summary>
        public void ApplyModeChanges(ChannelModeInfo[] modes)
        {
            foreach (ChannelModeInfo mode in modes)
            {
                char symbol = mode.Mode switch
                {
                    ChannelMode.ChannelOperator => '@',
                    ChannelMode.HalfChannelOperator => '%',
                    ChannelMode.Voice => '+',
                    _ => '\0'
                };

                if (symbol == '\0' || string.IsNullOrEmpty(mode.Parameter))
                    continue;

                User u = Users.GetUser(mode.Parameter);
                if (u == null)
                    continue;

                if (mode.Action == ModeAction.Add)
                    u.AddStatus(symbol);
                else
                    u.RemoveStatus(symbol);
            }

            Users.Refresh();
        }

        public void Part(string message)
        {
            Server.Connection.Sender.Part(message, Name);
            Users.Clear();
        }

        public void Part()
        {
            Part("Leaving");
        }

        public void UserPart(User user, string message)
        {
            if (user.Nick != Server.UserNick)
            {
                RemoveUser(user.Nick);
                OtherUserParted.Fire(this, new UserMessageEventArgs(user, message));
            }
            else
                UserParted.Fire(this, new EventArgs());
        }

        public void UserQuit(User user, string message)
        {
            //Make sure the user is in the channel
            User found = Users.GetUser(user.Nick);
            if (found == null) return;

            Users.Remove(found);
            UserQuitted.Fire(this, new UserMessageEventArgs(found, message));
        }

        /// <summary>
        ///   Checks if a user with the provided nick is in the channel.
        /// </summary>
        /// <param name="nick"> The nick to look for. </param>
        /// <returns> </returns>
        public bool HasUser(string nick)
        {
            return Users.Contains(User.FromNames(nick));
        }

        public void UserKick(User nick, string kickee, string reason)
        {
            RemoveUser(kickee);

            OnKick?.Invoke(nick, kickee, reason);
        }

        public void Say(string message)
        {
            Server.Connection.Sender.PublicMessage(Name, message);
            MessagedChannel.Fire(this,
                new UserMessageEventArgs(Users.GetUser(Server.UserNick), message));
        }

        public void Act(string message)
        {
            Server.Connection.Sender.Action(Name, message);
            OnAction.Fire(this, new UserMessageEventArgs(Users.GetUser(Server.UserNick), message));
        }

        // NAMES replies for this channel arrive as one or more chunks terminated by RPL_ENDOFNAMES.
        // Buffer them here, per channel, so interleaved replies for other channels cannot contaminate it.
        private readonly List<User> _pendingNames = new List<User>();

        /// <summary>
        ///   Buffers a chunk of NAMES nicks for this channel until the full list has been received.
        /// </summary>
        public void AddPendingNames(string[] nicks)
        {
            foreach (string nick in nicks)
            {
                User u = User.FromNames(nick);
                if (u != null)
                    _pendingNames.Add(u);
            }
        }

        /// <summary>
        ///   Replaces the channel's user list with the buffered NAMES list and clears the buffer.
        /// </summary>
        public void CommitPendingNames()
        {
            LoadNewNames(new List<User>(_pendingNames));
            _pendingNames.Clear();
        }

        /// <summary>
        ///   Loads a new list of user that replaces the old list
        /// </summary>
        /// <param name="users">The list of users</param>
        public void LoadNewNames(List<User> users)
        {
            Users.NotifyUpdate = false;
            Users.Clear();
            foreach (User nick in users)
            {
                if (nick != null)
                    AddNick(nick);
            }
            Users.NotifyUpdate = true;
            Users.Refresh();

            OnReceivedNames.Fire(this, new DataEventArgs<UserList>(new UserList(users)));
        }
    }
}
