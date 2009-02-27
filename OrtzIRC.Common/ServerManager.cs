namespace OrtzIRC.Common
{
    using System.Collections.Generic;

    public class ServerManager
    {
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

            return newServer;
        }
    }
}
