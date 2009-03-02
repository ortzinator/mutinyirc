namespace OrtzIRC
{
    using System;
    using OrtzIRC.Common;

    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs(Server server)
        {
            Server = server;
        }

        public Server Server { get; private set; }
    }
}
