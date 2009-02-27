namespace OrtzIRC
{
    using System;
    using System.Windows.Forms;
    using FlamingIRC;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    public delegate void ChannelFormAddLineEventHandler(string Text);

    public partial class ChannelForm : Form
    {
        private Server Server;

        public event ChannelFormAddLineEventHandler LineAdded;

        public ChannelForm(Channel channel)
        {
            InitializeComponent();

            Server = channel.Server;
            Channel = channel;

            Text = channel.Name;

            Channel.OnMessage += Channel_OnMessage;
            Channel.OnAction += Channel_OnAction;
            Channel.OnShowTopic += Channel_OnShowTopic;
            Channel.OnJoin += Channel_OnJoin;
            Channel.OnUserPart += Channel_OnPartOther;
            Channel.OnUserQuit += Channel_OnUserQuit;
            Channel.OnNick += Channel_OnNick;
            Channel.OnKick += Channel_OnKick;
            Channel.MessagedChannel += Channel_MessagedChannel;
            Server.Disconnected += Server_Disconnected;

            commandTextBox.CommandEntered += commandTextBox_CommandEntered;

            nickListBox.UserList = Channel.NickList;

            commandTextBox.Focus();
        }

        public Channel Channel { get; private set; }

        private void Channel_MessagedChannel(object sender, DataEventArgs<string> e)
        {
            AddLine(Server.UserNick + ": " + e.Data);
        }

        private void commandTextBox_CommandEntered(object sender, DataEventArgs<string> e)
        {
            if (e.Data.StartsWith("/"))
            {
                string[] exploded = e.Data.Split(new Char[] {' '});
                string name = exploded[0].TrimStart('/');
                string[] parameters = new string[exploded.Length - 1];
                Array.Copy(exploded, 1, parameters, 0, exploded.Length - 1); //Removing the first element
                PluginManager.GetCommandInstance(Channel, name, parameters);
            }
            else
            {
                string[] parameters = e.Data.Split(new Char[] {' '});
                PluginManager.GetCommandInstance(Channel, "say", parameters);
            }
        }

        private void Server_Disconnected()
        {
            Close();
        }

        private void Channel_OnKick(User nick, string kickee, string reason)
        {
            AddLine("-- Kick: (" + nick.Nick + ") was kicked by (" + kickee + ") " + reason);
        }

        private void Channel_OnNick(User nick, string newNick)
        {
            AddLine("-- Nick: (" + nick.Nick + ") is now known as (" + newNick + ")");
        }

        private void Channel_OnUserQuit(User nick, string message)
        {
            AddLine("-- Quit: (" + nick.Nick + ") (" + nick.HostMask + ") " + message);
        }

        private void Channel_OnPartOther(User nick, string message)
        {
            if (message != String.Empty)
                AddLine("-- Parted: (" + nick.Nick + ") (" + nick.HostMask + ") " + message);
            else
                AddLine("-- Parted: (" + nick.Nick + ") (" + nick.HostMask + ")");
        }

        private void Channel_OnJoin(User nick)
        {
            AddLine("-- Joined: (" + nick.Nick + ") (" + nick.HostMask + ")");
        }

        private void Channel_OnShowTopic(string topic)
        {
            AddLine("topic: (" + topic + ")");
        }

        private void Channel_OnAction(User nick, string message)
        {
            AddLine("-- " + nick.Nick + " " + message);
        }

        private void Channel_OnMessage(User nick, string message)
        {
            AddLine(nick.NamesLiteral + ": " + message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Channel.Part("Leaving"); //TODO: Get a default part message
        }

        public void AddLine(string line)
        {
            channelOutputBox.AppendLine(line);

            if (LineAdded != null)
                LineAdded(line);
        }
    }
}