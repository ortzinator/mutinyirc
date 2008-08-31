using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OrtzIRC
{
    public partial class ChannelForm : Form
    {
        private Server Server;
        private Channel Channel;
        private string ChannelName;

        public ChannelForm(Channel channel, Server server)
        {
            InitializeComponent();

            Server = server;
            Channel = channel;
            ChannelName = channel.Name;

            this.Text = channel.Name;

            Channel.OnReceivedNames += new ReceivedNamesEventHandler(Channel_OnReceivedNames);

            //var tempBinding = new Binding("Items", Channel.NickList, "NamesLiteral");
            //nickListBox.DataBindings.Add(tempBinding);
            //this.nickListBox.DataSource = channel.NickList;
            this.nickListBox.DisplayMember = "NamesLiteral";

            Channel.OnMessage += new ChannelMessageEventHandler(Channel_OnMessage);
            Channel.OnAction += new ChannelMessageEventHandler(Channel_OnAction);
            Channel.OnShowTopic += new TopicShowEventHandler(Channel_OnShowTopic);
            Channel.OnJoin += new ChannelJoinEventHandler(Channel_OnJoin);
            Channel.OnUserPart += new ChannelPartOtherEventHandler(Channel_OnPartOther);
            Channel.OnUserQuit += new ChannelQuitEventHandler(Channel_OnUserQuit);
            Channel.OnNick += new Server_NickEventHandler(Channel_OnNick);
            Channel.OnKick += new ChannelKickEventHandler(Channel_OnKick);

            this.commandTextBox.Focus();
        }

        void Channel_OnKick(Nick nick, string kickee, string reason)
        {
            this.AddLine("-- Kick: (" + nick.Name + ") was kicked by (" + kickee + ") " + reason);
        }

        void Channel_OnReceivedNames(List<Nick> nickList)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                nickListBox.Items.Clear();

                foreach (Nick nick in nickList)
                {
                    nickListBox.Items.Add(nick);
                }
            });
        }

        void Channel_OnNick(Nick nick, string newNick)
        {
            this.AddLine("-- Nick: (" + nick.Name + ") is now known as (" + newNick + ")");
        }

        void Channel_OnUserQuit(Nick nick, string message)
        {
            this.AddLine("-- Quit: (" + nick.Name + ") (" + nick.HostMask + ") " + message);
        }

        void Channel_OnPartOther(Nick nick, string message)
        {
            if (message != String.Empty)
                this.AddLine("-- Parted: (" + nick.Name + ") (" + nick.HostMask + ") " + message);
            else
                this.AddLine("-- Parted: (" + nick.Name + ") (" + nick.HostMask + ")");
        }

        void Channel_OnJoin(Nick nick)
        {
            this.AddLine("-- Joined: (" + nick.Name + ") (" + nick.HostMask + ")");
        }

        void Channel_OnShowTopic(string topic)
        {
            this.AddLine("topic: (" + topic + ")");
        }

        void Channel_OnAction(Nick nick, string message)
        {
            this.AddLine("-- " + nick.Name + " " + message);
        }

        void Channel_OnMessage(Nick nick, string message)
        {
            this.AddLine(nick.NamesLiteral + ": " + message);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Server.Connection.Sender.Part(ChannelName);
        }

        public void AddLine(string line)
        {
            channelOutputBox.AddLine(line);
        }

        protected override void OnResize(EventArgs e)
        {
            channelOutputBox.ScrollToBottom();
            base.OnResize(e);
        }
    }
}
