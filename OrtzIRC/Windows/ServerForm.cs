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
using OrtzIRC.Common;

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

            ParentServer.Registered += new RegisteredEventHandler(ParentServer_OnRegistered);
            ParentServer.Disconnected += new DisconnectedEventHandler(ParentServer_OnDisconnected);
            ParentServer.PublicMessage += new Server_ChannelMessageEventHandler(ParentServer_OnPublicMessage);
            ParentServer.Action += new Server_ChannelMessageEventHandler(ParentServer_OnAction);
            ParentServer.JoinSelf += new EventHandler<DataEventArgs<Channel>>(ParentServer_OnJoinSelf);
            ParentServer.JoinOther += ParentServer_OnJoinOther;
            ParentServer.Part += new Server_ChannelMessageEventHandler(ParentServer_OnPart);
            ParentServer.ConnectFail += new Server_ConnectFailedEventHandler(ParentServer_OnConnectFail);
            ParentServer.PrivateNotice += new Server_PrivateNoticeEventHandler(ParentServer_OnPrivateNotice);
            ParentServer.GotTopic += new Server_TopicRequestEventHandler(ParentServer_OnGotTopic);
            ParentServer.RawMessageReceived += new RawMessageReceivedEventHandler(ParentServer_OnRawMessageReceived);
            ParentServer.Error += new ErrorMessageEventHandler(ParentServer_OnError);
            ParentServer.Kick += new Server_KickEventHandler(ParentServer_OnKick);

            this.commandTextBox.Focus();
        }

        void ParentServer_OnKick(Nick nick, Channel chan, string kickee, string reason)
        {
            chan.UserKick(nick, kickee, reason);
        }

        void ParentServer_OnPart(Nick nick, Channel chan, string message)
        {
            chan.UserPart(nick, message);
        }

        void ParentServer_OnJoinOther(Nick nick, Channel chan)
        {
            chan.UserJoin(nick);
        }

        void ParentServer_OnError(ReplyCode code, string message)
        {
            this.serverOutputBox.AddLine(code.ToString() + " " + message);
        }

        void ParentServer_OnRawMessageReceived(string message)
        {
            //this.serverOutputBox.AddLine(message);
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
            ParentServer.Connection.Sender.Join("##csharp");
            this.Invoke((MethodInvoker)delegate
            {
                this.Text = "Status: " + ParentServer.UserNick + " on " + ParentServer.Description +
                " (" + ParentServer.URI + ":" + ParentServer.Port + ")";
            });
        }

        private void ParentServer_OnJoinSelf(object sender, DataEventArgs<Channel> e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ChannelForm newChan = new ChannelForm(e.Data, ParentServer);
                newChan.MdiParent = this.MdiParent;
                newChan.Show();
                ChannelFormList.Add(newChan);
                newChan.AddLine("Joined: " + e.Data.Name);
            });
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

        public Form GetCurrentWindow()
        {
            if (this.Focused)
            {
                return this;
            }

            foreach (ChannelForm chan in ChannelFormList)
            {
                if (chan.Focused)
                {
                    return chan;
                }
            }
            return null;
        }
    }
}