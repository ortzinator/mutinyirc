using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public class Channel
    {
        public Server Server { get; private set; }
        public ChannelForm ChannelView { get; private set; }
        //public List<Nick> BanList { get; private set; }
        //public ModeCollection Mode { get; private set; }
        public string Key { get; private set; }
        public int Limit { get; private set; }
        public string Name { get; private set; }
        //public List<Nick> NickList { get; private set; }
        //public Topic Topic { get; private set; }
        public bool InChannel { get; private set; }

        public Channel(Server parent, string Name)
        {
            Server = parent;

            ChannelView = new ChannelForm(Server, Name);
            ChannelView.MdiParent = parent.ServerView.MdiParent;
            ChannelView.Show();
            ChannelView.Focus();
        }

        public void Act()
        {
            throw new System.NotImplementedException();
        }

        public void Say()
        {
            throw new System.NotImplementedException();
        }

        public void Method()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeTopic(string topic)
        {
            throw new System.NotImplementedException();
        }

        public void AppendLine(string line)
        {
            ChannelView.AppendLine(line);
        }

        public void AddNick(string nick)
        {
            ChannelView.AddNick(nick);
        }

        public void ResetNicks()
        {
            ChannelView.ResetNicks();
        }
    }
}
