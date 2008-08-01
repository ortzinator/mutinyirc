using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    public class ChannelManager
    {
        public Dictionary<string, Channel> Channels { get; private set; }
        public Server Server { get; private set; }

        public Channel GetChannel(string channelName)
        {
            return Channels[channelName];
        }

        public bool InChannel(string channelName)
        {
            return Channels[channelName].InChannel;
        }

        internal void CloseAll()
        {
            throw new NotImplementedException();
        }

        public Channel NewChannel(string channelName)
        {
            Channel newChan = new Channel(this.Server, channelName);
            Channels.Add(channelName, newChan);
            return newChan;
        }
    }
}
