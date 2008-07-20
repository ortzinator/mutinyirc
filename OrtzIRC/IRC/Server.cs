using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC.IRC
{
    public class Server : Target
    {

        private string _uri;
        public string URI
        {
            get { return _uri; }
            set { _uri = value; }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public Server(string URI, string description, int port)
        {
            //this.uri = URI;
            this._description = description;
            this._port = port;
            //network.AddServer(this);
        }

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }
    }
}
