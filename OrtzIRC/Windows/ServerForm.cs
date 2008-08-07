using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sharkbite.Irc;
using System.Collections;
using FlamingIRC;

namespace OrtzIRC
{
    public partial class ServerForm : Form
    {
        public Server ParentServer { get; private set; }

        public List<ChannelForm> ChannelFormList { get; private set; }

        delegate void SetTextCallback(string text);
        delegate void NewChannelCallback(string channel);

        public ServerForm(Server parent)
        {
            InitializeComponent();

            this.serverOutputBox.AddLine("Connecting to " + parent.URI + "(" + parent.Port.ToString() + ")");

            ChannelFormList = new List<ChannelForm>();

            ParentServer = parent;

            ParentServer.OnRegistered += new RegisteredEventHandler(ParentServer_OnRegistered);
            ParentServer.OnDisconnected += new DisconnectedEventHandler(ParentServer_OnDisconnected);
            ParentServer.OnPublicMessage += new Server_ChannelMessageEventHandler(ParentServer_OnPublicMessage);
            ParentServer.OnAction += new Server_ChannelMessageEventHandler(ParentServer_OnAction);
            ParentServer.OnJoinSelf += new Server_SelfJoinEventHandler(ParentServer_OnJoinSelf);
            ParentServer.OnConnectFail += new Server_ConnectFailedEventHandler(ParentServer_OnConnectFail);
            ParentServer.OnPrivateNotice += new Server_PrivateNoticeEventHandler(ParentServer_OnPrivateNotice);
            ParentServer.OnGotTopic += new Server_TopicRequestEventHandler(ParentServer_OnGotTopic);

            this.commandTextBox.Focus();
        }

        void ParentServer_OnGotTopic(Channel chan, string topic)
        {
            chan.ShowTopic(topic);
        }

        void ParentServer_OnPrivateNotice(Nick nick, string notice)
        {
            this.serverOutputBox.AddLine("-" + nick.Name + ":" + notice + "-");
        }

        void ParentServer_OnConnectFail(string message)
        {
            this.serverOutputBox.AddLine("Could not connect");
        }

        void ParentServer_OnAction(Nick nick, Channel chan, string message)
        {
            chan.NewAction(nick, message);
        }

        void ParentServer_OnRegistered()
        {
            ParentServer.Connection.Sender.Join("#ortzirc");
        }

        void ParentServer_OnJoinSelf(Channel chan)
        {
            ChannelForm newChan = new ChannelForm(chan, ParentServer);
            this.Invoke((MethodInvoker)delegate
            {
                newChan.MdiParent = this.MdiParent;
                newChan.Show();
            });

            ChannelFormList.Add(newChan);
            newChan.AddLine("Joined: " + chan.Name);
        }

        void ParentServer_OnPublicMessage(Nick nick, Channel chan, string message)
        {
            chan.NewMessage(nick, message);
        }

        void ParentServer_OnDisconnected()
        {
            foreach (ChannelForm chan in ChannelFormList)
            {
                chan.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (ParentServer.Connection.Connected)
            {
                if (MessageBox.Show("Do you wish to disconnect from the server?", "", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ParentServer.Connection.Disconnect("Quitan");
                }
                else
                {
                    e.Cancel = true;
                }
            }

            base.OnFormClosing(e);
        }
    }
}