namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using OrtzIRC.Common;

    public class ServerManager
    {
        private static ServerManager _instance;

        public static List<Server> ServerList { get; private set; }

        public static ServerManager Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new ServerManager();
                    ServerList = new List<Server>();
                }

                return _instance;
            }
        }

        public Server Create(ServerSettings settings)
        {
            Server newServer = new Server(settings);
            ServerList.Add(newServer);
            return newServer;
        }

        public Server Create(string uri, string description, int port, bool ssl)
        {
            Server newServer = new Server(uri, description, port, ssl);
            ServerList.Add(newServer);

            return newServer;
        }
    }
}
