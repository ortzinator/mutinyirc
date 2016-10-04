using System;
using System.Collections.Generic;
using FlamingIRC;

namespace OrtzIRC.Common
{
    public delegate void ChannelKickEventHandler(User nick, string kickee, string reason);

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
        ///   At the moment, this is kept up to date by requesting a NAMES list for the channel 
        ///   whenever someone joins, parts, quits, or their mode is changed (and thus their prefix symbol).
        /// </remarks>
        public UserList Users { get; set; }

        /// <summary>
        ///   Returns true if the user is in the channel
        /// </summary>
        public bool Joined
        {
            get { return Users.Count > 0; }
        }

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
            OnJoin.Fire(this, new UserEventArgs(nick));
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
                OtherUserParted.Fire(this, new UserMessageEventArgs(user, message));
            else
                UserParted.Fire(this, new EventArgs());
        }

        public void UserQuit(User user, string message)
        {
            //Make sure the user is in the channel
            foreach (User u in Users)
            {
                if (user.Nick != u.Nick) continue;
                UserQuitted.Fire(this, new UserMessageEventArgs(u, message));
            }
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
            Server.Connection.Sender.Names(Name);

            OnKick?.Invoke(nick, kickee, reason);
        }

        public void Say(string message)
        {
            Server.Connection.Sender.PublicMessage(Name, message);
            MessagedChannel.Fire(this, new UserMessageEventArgs(Users.GetUser(Server.UserNick), message));
        }

        public void Act(string message)
        {
            Server.Connection.Sender.Action(Name, message);
            MessagedChannel.Fire(this, new UserMessageEventArgs(Users.GetUser(Server.UserNick), message));
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