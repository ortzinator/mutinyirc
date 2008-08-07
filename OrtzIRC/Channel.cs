using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace OrtzIRC
{
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

        private delegate void SyncCallback();

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
    }
}
