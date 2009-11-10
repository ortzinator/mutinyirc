namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using System;

    public class ServerManager
    {
        public event EventHandler<ServerEventArgs> ServerAdded;
        public event EventHandler<ServerEventArgs> ServerRemoved;

        private static ServerManager instance;

        public List<Server> ServerList { get; private set; }

        public static ServerManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServerManager { ServerList = new List<Server>() };

                return instance;
            }
        }

        public Server Create(ServerSettings settings)
        {
            var newServer = new Server(settings);
            ServerList.Add(newServer);

            ServerAdded.Fire(this, new ServerEventArgs(newServer));

            return newServer;
        }

        public void Remove(Server ntw)
        {
            if (ServerList.Remove(ntw))
                ServerRemoved.Fire(this, new ServerEventArgs(ntw));
        }

        public void DisconnectAll()
        {
            foreach (var server in ServerList)
            {
                if (server.IsConnected)
                    server.Disconnect();
            }
        }
    }
}
