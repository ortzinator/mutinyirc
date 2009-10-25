namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using FlamingIRC;
    using System;
    using System.Diagnostics;

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

        private List<User> tempNicks = new List<User>();
        private void Server_OnNames(string channel, string[] nicks, bool last)
        {
            //if (!InChannel(channel)) return;

            Channel chan = GetChannel(channel);
            if (!recievingNames)
            {
                recievingNames = true;
            }

                
            foreach (string nick in nicks)
            {
                tempNicks.Add(User.FromNames(nick));
            }

            Trace.WriteLine("Added chunk of " + nicks.Length + " names", "Names");

            if (!last) return;

            chan.NickList.NotifyUpdate = false;
            chan.NickList.Clear();
            foreach (User nick in tempNicks)
            {
                chan.NickList.Add(nick);
            }
            chan.NickList.NotifyUpdate = true;
            chan.NickList.Refresh();
            //Trace.WriteLine("Refreshed channel nick list", "Names");
            recievingNames = false;
            tempNicks.Clear();
        }

        public Channel GetChannel(string channelName)
        {
            //if (!InChannel(channelName))
            //   return null;

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

            if (!Rfc2812Util.IsValidChannelName(channelName))
                return null;

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
