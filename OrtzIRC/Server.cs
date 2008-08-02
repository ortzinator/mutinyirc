using System;
using System.Collections.Generic;
using System.Text;
using Sharkbite.Irc;
using OrtzIRC;
using System.Windows.Forms;

namespace OrtzIRC
{
    public class Server
    {
        public string URI { get; set; }
        public string Description { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public Connection Connection { get; private set; }
        public string Nick { get; private set; }
        public ServerForm ServerView { get; private set; }

        public ChannelManager ChanManager { get; private set; }

        public Server(ServerSettings settings, Form parent)
        {
            this.URI = settings.URI;
            this.Description = settings.Description;
            this.Port = settings.Port;
            this.SSL = settings.Ssl;
            //network.AddServer(this);

            this.ChanManager = new ChannelManager(this);

            this.Nick = "OrtzIRC";

            this.ServerView = new ServerForm(this);
            this.ServerView.MdiParent = parent;
            this.ServerView.Show();
            this.ServerView.Focus();
            this.ServerView.AppendLine("Connecting...");

            ConnectionArgs args = new ConnectionArgs(this.Nick, settings.URI);
            this.Connection = new Connection(args, false, false);

            Connection.Listener.OnJoin += new JoinEventHandler(OnJoin);
            Connection.Listener.OnPart += new PartEventHandler(OnPart);
            Connection.Listener.OnPublic += new PublicMessageEventHandler(OnPublic);
            Connection.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
            Connection.Listener.OnNames += new NamesEventHandler(OnNames);
            Connection.Listener.OnChannelModeChange += new ChannelModeChangeEventHandler(OnChannelModeChange);
            Connection.Listener.OnError += new ErrorMessageEventHandler(OnError);

            Connection.OnConnectSuccess += new ConnectEventHandler(OnConnectSuccess);

            try
            {
                Connection.Connect();
            }
            catch (Exception e)
            {
                this.AppendLine("Could not connect to server: " + e.Message);
            }
        }

        void OnConnectSuccess()
        {
            ServerView.AppendLine("Connected!");
        }

        private void AppendLine(string line)
        {
            ServerView.AppendLine(line);
        }

        private void NewChannel(string channelName)
        {
            //TODO: de-crappify this
            ChanManager.NewChannel(channelName);
            ChanManager.GetChannel(channelName).AppendLine("Joined: " + channelName);
        }

        private void JoinChannel(string channel)
        {
            ChanManager.NewChannel(channel);
            Connection.Sender.Join(channel);
        }

        private void OnPublic(UserInfo user, string channel, string message)
        {
            ChanManager.GetChannel(channel).AppendLine("<" + user.Nick + "> " + message);
        }

        private void OnNames(string channel, string[] nicks, bool last)
        {
            ChanManager.OnNames(channel, nicks, last);
        }

        private void OnJoin(UserInfo user, string channel)
        {
            if (user.Nick == Nick)
            {
                if (ChanManager.InChannel(channel))
                {
                    ChanManager.GetChannel(channel).AppendLine("Joined: " + channel);
                }
                else
                {
                    //this.Invoke(new NewChannelCallback(NewChannel), new object[] { channel });
                    //this.NewChannel(channel);
                }

            }
            else
            {
                ChanManager.GetChannel(channel).AppendLine("<<" + user.Nick + ">> has joined " + channel);
                Connection.Sender.Names(channel);
            }
        }

        private void OnPart(UserInfo user, string channel, string reason)
        {
            ChanManager.GetChannel(channel).AppendLine("<<" + user.Nick + ">> has parted " + channel);
            Connection.Sender.Names(channel);
        }

        private void OnRegistered()
        {
            JoinChannel("#ortzirc");
        }

        private void OnChannelModeChange(UserInfo who, string channel, ChannelModeInfo[] modes, string raw)
        {
            ChanManager.GetChannel(channel).AppendLine("<<" + who.Nick + ">> sets mode (" + raw + ")");
            Connection.Sender.Names(channel);
        }

        private void OnError(ReplyCode code, string message)
        {
            this.AppendLine(message);
        }
    }
}
