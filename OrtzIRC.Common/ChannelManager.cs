namespace OrtzIRC
{
    using System.Collections.Generic;
    using FlamingIRC;
    using OrtzIRC.Common;
    using System;

    public sealed class ChannelManager
    {
        private bool recievingNames;

        public ChannelManager(Server server)
        {
            Server = server;
            Channels = new Dictionary<string, Channel>();

            Server.OnNames += Server_OnNames;
            Server.OnNick += Server_OnNick;
        }

        public Dictionary<string, Channel> Channels { get; private set; }
        public Server Server { get; private set; }

        public static event EventHandler<ChannelEventArgs> ChannelCreated;
        public static event EventHandler<ChannelEventArgs> ChannelRemoved;

        private void Server_OnNick(User nick, string newNick)
        {
            foreach (KeyValuePair<string, Channel> item in Channels)
            {
                if (item.Value.HasUser(nick.Nick))
                {
                    item.Value.NickChange(nick, newNick);
                }
            }
        }

        private void Server_OnNames(string channel, string[] nicks, bool last)
        {
            if (InChannel(channel))
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
                    GetChannel(channel).NickList.Refresh();
                } 
            }
        }

        public Channel GetChannel(string channelName)
        {
            return Channels[channelName];
        }

        public bool InChannel(string channelName)
        {
            if (Channels.ContainsKey(channelName))
            {
                if (Channels[channelName].Joined)
                {
                    return true;
                }

                TriggerChannelRemoved(Channels[channelName]);
                Channels.Remove(channelName);
                //TODO: Is this all that needs to be done?
            }
            return false;
        }

        public Channel Create(string channelName)
        {
            if (Channels.ContainsKey(channelName))
            {
                return Channels[channelName];
            }

            Channel newChan = new Channel(Server, channelName);
            Channels.Add(channelName, newChan);
            TriggerChannelCreated(newChan);

            return newChan;
        }

        private void TriggerChannelCreated(Channel chan)
        {
            ChannelCreated.Fire(this, new ChannelEventArgs(chan));
        }

        private void TriggerChannelRemoved(Channel chan)
        {
            ChannelRemoved.Fire(this, new ChannelEventArgs(chan));
        }
    }
}
