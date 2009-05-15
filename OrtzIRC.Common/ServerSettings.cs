namespace OrtzIRC.Common
{
    public class ServerSettings
    {
        public ServerSettings(string uri, string description, int port, bool ssl)
        {
            Uri = uri;
            Description = description;
            Port = port;
            Ssl = ssl;
        }

        public string Uri { get; private set; }
        public string Description { get; private set; }
        public int Port { get; private set; }
        public bool Ssl { get; private set; }
    }
}
