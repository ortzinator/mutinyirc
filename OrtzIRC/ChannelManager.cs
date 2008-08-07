using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    public class ChannelManager
    {
        public Dictionary<string, Channel> Channels { get; private set; }
        public Server Server { get; private set; }

        public ChannelManager(Server server)
        {
            this.Server = server;
            this.Channels = new Dictionary<string, Channel>();
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
            Channel newChan = new Channel(this.Server, channelName);
            Channels.Add(channelName, newChan);
            return newChan;
        }

        private bool recievingNames = false;
        public void OnNames(string channel, string[] nicks, bool last)
        {
            if (!recievingNames)
            {
                GetChannel(channel).ResetNicks();
                recievingNames = true;
            }

            foreach (string nick in nicks)
            {
                GetChannel(channel).AddNick(nick);
            }

            if (last)
            {
                recievingNames = false;
                //Server.AppendLine("NAMES received for " + channel);
                //GetChannel(channel).ChannelView.RefreshNicks();
            }
        }
    }
}
