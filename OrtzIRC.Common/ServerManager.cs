namespace OrtzIRC.Common
{
    using System.Collections.Generic;

    public delegate void ServerCreatedEventHandler(Server Ntw);
    public delegate void ServerRemovedEventHandler(Server Ntw);

    public class ServerManager
    {
        public event ServerCreatedEventHandler ServerCreated;
        public event ServerRemovedEventHandler ServerRemoved;

        private static ServerManager instance;

        public static List<Server> ServerList { get; private set; }

        public static ServerManager Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new ServerManager();
                    ServerList = new List<Server>();
                }

                return instance;
            }
        }

        public Server Create(string uri, string description, int port, bool ssl)
        {
            Server newServer = new Server(uri, description, port, ssl);
            ServerList.Add(newServer);

            if (ServerCreated != null)
                ServerCreated(newServer);

            return newServer;
        }

        public void Remove(Server Ntw)
        {
            ServerList.Remove(Ntw);

            if (ServerRemoved != null)
                ServerRemoved(Ntw);
        }
    }
}
