namespace OrtzIRC
{
    using System.Collections.Generic;

    public delegate void ChannelMessageEventHandler(Nick nick, string message);
    public delegate void TopicShowEventHandler(string topic);
    public delegate void ChannelJoinEventHandler(Nick nick);
    public delegate void ChannelPartOtherEventHandler(Nick nick, string message);
    public delegate void ChannelQuitEventHandler(Nick nick, string message);
    public delegate void ReceivedNamesEventHandler(List<Nick> nickList);
    public delegate void ChannelKickEventHandler(Nick nick, string kickee, string reason);

    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public class Channel
    {
        public Server Server { get; private set; }
        //public List<Nick> BanList { get; private set; }
        //public ModeCollection Mode { get; private set; }
        public string Key { get; private set; }
        public int Limit { get; private set; }
        public string Name { get; private set; }
        public List<Nick> NickList { get; private set; }
        //public Topic Topic { get; private set; }

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

            NickList = new List<Nick>();
        }

        public void AddNick(Nick nick)
        {
            //ChannelView.AddNick(nick);
            NickList.Add(nick);
        }

        public void ResetNicks()
        {
            if (OnReceivedNames != null)
                OnReceivedNames(NickList);
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal void NewMessage(Nick nick, string message)
        {
            foreach (Nick n in NickList)
            {
                if (nick.Name == n.Name)
                {
                    if (OnMessage != null)
                        OnMessage(n, message);
                }
            }
        }

        internal void NewAction(Nick nick, string message)
        {
            foreach (Nick n in NickList)
            {
                if (nick.Name == n.Name)
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

        internal void UserJoin(Nick nick)
        {
            if (OnJoin != null)
                OnJoin(nick);
        }

        internal void UserPart(Nick nick, string message)
        {
            if (OnUserPart != null)
                OnUserPart(nick, message);
        }

        internal void UserQuit(Nick nick, string message)
        {
            foreach (Nick n in NickList)
            {
                if (nick.Name == n.Name)
                {
                    if (OnUserQuit != null)
                        OnUserQuit(n, message);
                }
            }
        }

        internal void NickChange(Nick nick, string newNick)
        {
            foreach (Nick n in NickList)
            {
                if (nick.Name == n.Name)
                {
                    if (OnNick != null)
                        OnNick(n, newNick);
                    Server.Connection.Sender.Names(this.Name);
                }
            }
        }

        public bool HasUser(string nick)
        {
            foreach (Nick n in NickList)
            {
                if (nick == n.Name)
                {
                    return true;
                }
            }
            return false;
        }

        internal void UserKick(Nick nick, string kickee, string reason)
        {
            Server.Connection.Sender.Names(this.Name);
            if (OnKick != null)
                OnKick(nick, kickee, reason);
        }
    }
}
