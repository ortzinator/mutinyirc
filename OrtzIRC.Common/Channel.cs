namespace OrtzIRC.Common
{
    using FlamingIRC;
    using System.Collections.Generic;
    using System;

    public delegate void ChannelMessageEventHandler(User nick, string message);
    public delegate void TopicShowEventHandler(string topic);
    public delegate void ChannelJoinEventHandler(User nick);
    public delegate void ChannelPartOtherEventHandler(User nick, string message);
    public delegate void ChannelQuitEventHandler(User nick, string message);
    public delegate void ReceivedNamesEventHandler(UserList nickList);
    public delegate void ChannelKickEventHandler(User nick, string kickee, string reason);

    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public class Channel
    {
        public Server Server { get; private set; }
        public string Key { get; private set; }
        public int Limit { get; private set; }
        public string Name { get; private set; }
        public UserList NickList { get; private set; }

        /// <summary>
        /// Returns true if the user is in the channel
        /// </summary>
        public bool Joined { get; private set; }

        public ChannelInfo Info
        {
            get
            {
                return new ChannelInfo(this.Name);
            }
        }

        public event ChannelMessageEventHandler OnMessage;
        public event ChannelMessageEventHandler OnAction;
        public event TopicShowEventHandler OnShowTopic;
        public event ChannelJoinEventHandler OnJoin;
        public event ChannelPartOtherEventHandler OnUserPart;
        public event ChannelQuitEventHandler OnUserQuit;
        public event Server_NickEventHandler OnNick;
        public event ReceivedNamesEventHandler OnReceivedNames;
        public event ChannelKickEventHandler OnKick;

        public Channel(Server parent, string name)
        {
            this.Server = parent;
            this.Name = name;

            NickList = new UserList();

            this.Joined = true;
        }

        public void AddNick(User nick)
        {
            NickList.Add(nick);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void NewMessage(User nick, string message)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    if (OnMessage != null)
                        OnMessage(n, message);
                }
            }
        }

        public void NewAction(User nick, string message)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    if (OnAction != null)
                        OnAction(n, message);
                }
            }
        }

        public void ShowTopic(string topic)
        {
            if (OnShowTopic != null)
                OnShowTopic(topic);
        }

        public void UserJoin(User nick)
        {
            if (OnJoin != null)
                OnJoin(nick);
        }

        public void Part(string message)
        {
            Server.Connection.Sender.Part(message, this.Name);
            this.Joined = false;
        }

        public void UserPart(User nick, string message)
        {
            if (nick.Nick != this.Server.UserNick && OnUserPart != null)
                OnUserPart(nick, message);
        }

        public void UserQuit(User nick, string message)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    if (OnUserQuit != null)
                        OnUserQuit(n, message);
                }
            }
        }

        public void NickChange(User nick, string newNick)
        {
            foreach (User n in NickList)
            {
                if (nick.Nick == n.Nick)
                {
                    if (OnNick != null)
                        OnNick(n, newNick);
                    Server.Connection.Sender.Names(this.Name);
                }
            }
        }

        public bool HasUser(string nick)
        {
            foreach (User n in NickList)
            {
                if (nick == n.Nick)
                {
                    return true;
                }
            }

            return false;
        }

        public void UserKick(User nick, string kickee, string reason)
        {
            Server.Connection.Sender.Names(this.Name);

            if (OnKick != null)
                OnKick(nick, kickee, reason);
        }
    }
}
