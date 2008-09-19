namespace OrtzIRC
{
    using System.Collections.Generic;
    using FlamingIRC;

    public class ChannelManager
    {
        public Dictionary<string, Channel> Channels { get; private set; }
        public Server Server { get; private set; }

        public ChannelManager(Server server)
        {
            this.Server = server;
            this.Channels = new Dictionary<string, Channel>();

            Server.OnNames += new NamesEventHandler(Server_OnNames);
            Server.OnNick += new Server_NickEventHandler(Server_OnNick);
        }

        void Server_OnNick(User nick, string newNick)
        {
            foreach (KeyValuePair<string, Channel> item in Channels)
            {
                if (item.Value.HasUser(nick.Nick))
                {
                    item.Value.NickChange(nick, newNick);
                }
            }
        }

        private bool recievingNames = false;
        void Server_OnNames(string channel, string[] nicks, bool last)
        {
            if (!recievingNames)
            {
                GetChannel(channel).NickList.Clear();
                recievingNames = true;
            }

            foreach (string nick in nicks)
            {
                GetChannel(channel).NickList.Add(User.FromNames(nick));
            }

            if (last)
            {
                recievingNames = false;
                GetChannel(channel).ResetNicks();
            }
        }

        public Channel GetChannel(string channelName)
        {
            return Channels[channelName];
        }

        public bool InChannel(string channelName)
        {
            return Channels.ContainsKey(channelName);
        }

        public Channel Create(string channelName)
        {
            if (Channels.ContainsKey(channelName))
            {
                return Channels[channelName];
            }
            else
            {
                Channel newChan = new Channel(this.Server, channelName);
                Channels.Add(channelName, newChan);
                return newChan;
            }
        }
    }
}
