namespace OrtzIRC
{
    using FlamingIRC;
    using System.Collections.Generic;

    public delegate void ChannelMessageEventHandler(User nick, string message);
    public delegate void TopicShowEventHandler(string topic);
    public delegate void ChannelJoinEventHandler(User nick);
    public delegate void ChannelPartOtherEventHandler(User nick, string message);
    public delegate void ChannelQuitEventHandler(User nick, string message);
    public delegate void ReceivedNamesEventHandler(List<User> nickList);
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
        public List<User> NickList { get; private set; }

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

            NickList = new List<User>();
        }

        public void AddNick(User nick)
        {
            NickList.Add(nick);
        }

        public void ResetNicks()
        {
            if (OnReceivedNames != null)
            {
                OnReceivedNames(NickList);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal void NewMessage(User nick, string message)
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

        internal void NewAction(User nick, string message)
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

        internal void ShowTopic(string topic)
        {
            if (OnShowTopic != null)
                OnShowTopic(topic);
        }

        internal void UserJoin(User nick)
        {
            if (OnJoin != null)
                OnJoin(nick);
        }

        internal void UserPart(User nick, string message)
        {
            if (OnUserPart != null)
                OnUserPart(nick, message);
        }

        internal void UserQuit(User nick, string message)
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

        internal void NickChange(User nick, string newNick)
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

        internal void UserKick(User nick, string kickee, string reason)
        {
            Server.Connection.Sender.Names(this.Name);

            if (OnKick != null)
                OnKick(nick, kickee, reason);
        }
    }
}
