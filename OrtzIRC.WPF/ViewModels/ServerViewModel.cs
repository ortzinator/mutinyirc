using System.ComponentModel;

namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using OrtzIRC.Common;
    using System.Windows.Threading;

    public class ServerViewModel : IrcViewModel
    {
        private Server _server;

        public ServerViewModel(Server server)
        {
            _server = server;
            Name = _server.Url;
            _server.RawMessageReceived += Server_RawMessageReceived;
            //_server.Registered += Server_Registered;
            //_server.JoinSelf += Server_JoinSelf;
            //_server.ConnectFailed += Server_ConnectFail;
            //_server.PrivateNotice += Server_PrivateNotice;
            //_server.ErrorMessageRecieved += Server_OnError;
            //_server.Connecting += Server_Connecting;
            //_server.Disconnected += Server_Disconnected;
            //_server.ConnectionLost += Server_ConnectionLost;
            //_server.ConnectCancelled += Server_ConnectCancelled;
            //_server.NickError += Server_NickError;
            //_server.PartSelf += Server_PartSelf;
        }

        private void Server_PartSelf(object sender, PartEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_NickError(object sender, NickErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_ConnectCancelled(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_ConnectionLost(object sender, DisconnectEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_Disconnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_Connecting(object sender, CancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_OnError(object sender, ErrorMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_PrivateNotice(object sender, UserMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_ConnectFail(object sender, ConnectFailedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_JoinSelf(object sender, DataEventArgs<Channel> e)
        {
            throw new NotImplementedException();
        }

        private void Server_Registered(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Server_RawMessageReceived(object sender, DataEventArgs<string> e)
        {
            ChatLines.Add(new ChatItemViewModel(DateTime.Now, e.Data));
        }
    }
}
