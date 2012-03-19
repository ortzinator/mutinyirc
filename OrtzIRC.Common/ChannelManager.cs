namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using FlamingIRC;
    using System;
    using System.Diagnostics;

    //TODO: Refactor (SRP!)
    public sealed class ChannelManager
    {
        private bool recievingNames;

        public ChannelManager(Server server)
        {
            Server = server;
            Channels = new Dictionary<string, Channel>();

            HookEvents();
        }

        public void HookEvents()
        {
            Server.OnNames += Server_OnNames;
            Server.OnNick += Server_OnNick;
        }

        public void UnhookEvents()
        {
            Server.OnNames -= Server_OnNames;
            Server.OnNick -= Server_OnNick;
        }

        public Dictionary<string, Channel> Channels { get; private set; }
        public Server Server { get; private set; }

        public static event EventHandler<ChannelEventArgs> ChannelCreated;
        public static event EventHandler<ChannelEventArgs> ChannelRemoved;

        private void Server_OnNick(object sender, NickChangeEventArgs ea)
        {
            foreach (KeyValuePair<string, Channel> item in Channels)
            {
                if (item.Value.HasUser(ea.User.Nick))
                {
                    item.Value.NickChange(ea.User, ea.NewNick);
                }
            }
        }

        private List<User> tempNicks = new List<User>();
        private void Server_OnNames(string channel, string[] nicks, bool last)
        {
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
                if (nick != null)
                    chan.AddNick(nick);
            }
            chan.NickList.NotifyUpdate = true;
            chan.NickList.Refresh();
            //Trace.WriteLine("Refreshed channel nick list", "Names");
            recievingNames = false;
            tempNicks.Clear();
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
                if (!Channels.Remove(channelName))
                    Debug.WriteLine("Failed to remove channel");
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

            var newChan = new Channel(Server, channelName);
            Channels.Add(channelName, newChan);
            newChan.UserParted += channelUserParted;
            TriggerChannelCreated(newChan);

            return newChan;
        }

        private void channelUserParted(object sender, EventArgs e)
        {
            var chan = (Channel)sender;

            Channels.Remove(chan.Name);
            TriggerChannelRemoved(chan);

            chan.UserParted -= channelUserParted;
        }

        private void TriggerChannelCreated(Channel chan)
        {
            ChannelCreated.Fire(this, new ChannelEventArgs(chan));
        }

        private void TriggerChannelRemoved(Channel chan)
        {
            ChannelRemoved.Fire(this, new ChannelEventArgs(chan));
        }

        public void OnQuit(User user, string reason)
        {
            foreach (KeyValuePair<string, Channel> pair in Channels)
            {
                pair.Value.UserQuit(user, reason);
            }
        }
    }
}
