using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace FlamingIRC
{
    /// <summary>
    /// A client connection to a server
    /// </summary>
    public class Connection
    {
        private Dictionary<string, Channel> _joinedChannels;

        public Dictionary<string, Channel> Channels
        {
            get { return _joinedChannels; }
        }

        public Dictionary<string, Channel> JoinedChannels
        {
            get { return _joinedChannels; }
            set { _joinedChannels = value; }
        }
        private Server _server;
        private bool _debug;

        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public void MessageParser()
        {
            throw new System.NotImplementedException();
        }

        public void Connect()
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        private TcpClient _client;

        public Connection(Server server)
        {
            throw new System.NotImplementedException();
        }

        private bool _connected;

        public bool Connected
        {
            get { return _connected; }
        }

    }
}
