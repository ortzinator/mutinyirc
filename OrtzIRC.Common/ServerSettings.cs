namespace OrtzIRC.Common
{
    public class ServerSettings
    {
        public ServerSettings(string uri, string description, int port, bool ssl)
        {
            Uri = uri;
            Description = description;
            Ports = port;
            Ssl = ssl;
        }

        public ServerSettings() { }

        public string Uri { get; set; }
        public string Description { get; set; }
        public int Ports { get; set; }
        public bool Ssl { get; set; }
        public int Id { get; set; }
    }
}
