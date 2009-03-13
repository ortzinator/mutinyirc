namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using System;

    public class ServerManager
    {
        public event EventHandler<ServerEventArgs> ServerCreated;
        public event EventHandler<ServerEventArgs> ServerRemoved;

        private static ServerManager instance;

        public List<Server> ServerList { get; private set; }

        public static ServerManager Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new ServerManager {ServerList = new List<Server>()};
                }

                return instance;
            }
        }

        public Server Create(string uri, string description, int port, bool ssl)
        {
            Server newServer = new Server(uri, description, port, ssl);
            ServerList.Add(newServer);

            ServerCreated.Fire(this, new ServerEventArgs(newServer));

            return newServer;
        }

        public void Remove(Server ntw)
        {
            ServerList.Remove(ntw);

            ServerRemoved.Fire(this, new ServerEventArgs(ntw));
        }
    }
}
