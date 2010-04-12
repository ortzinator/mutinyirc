namespace OrtzIRC.Common
{
    using System;

    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs(Server server)
        {
            Server = server;
        }

        public Server Server { get; private set; }
    }
}
