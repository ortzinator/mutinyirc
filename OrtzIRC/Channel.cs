using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace OrtzIRC
{
    public delegate void ChannelMessageEventHandler(Nick nick, string message);
    public delegate void TopicShowEventHandler(string topic);

    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public class Channel : Target
    {
        public Server Server { get; private set; }
        //public List<Nick> BanList { get; private set; }
        //public ModeCollection Mode { get; private set; }
        public string Key { get; private set; }
        public int Limit { get; private set; }
        public string Name { get; private set; }
        public BindingList<string> NickList { get; private set; }
        //public Topic Topic { get; private set; }

        public event ChannelMessageEventHandler OnMessage;
        public event ChannelMessageEventHandler OnAction;
        public event TopicShowEventHandler OnShowTopic;

        public Channel(Server parent, string name)
        {
            this.Server = parent;
            this.Name = name;

            NickList = new BindingList<string>();
        }

        public void AddNick(string nick)
        {
            //ChannelView.AddNick(nick);
            NickList.Add(nick);
        }

        public void ResetNicks()
        {
            NickList.Clear();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void NewMessage(Nick nick, string message)
        {
            if (OnMessage != null)
                OnMessage(nick, message);
        }

        public void NewAction(Nick nick, string message)
        {
            if (OnAction != null)
                OnAction(nick, message);
        }

        public void ShowTopic(string topic)
        {
            if (OnShowTopic != null)
                OnShowTopic(topic);
        }
    }
}
