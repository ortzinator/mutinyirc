namespace OrtzIRC
{
    public sealed class IRCSettingsManager
    {
        private static IRCSettingsManager _instance;

        public static IRCSettingsManager Instance
        {
            get { return _instance ?? (_instance = new IRCSettingsManager()); }
        }

        private IRCSettingsManager()
        {
            //this is just here to make the class inconstructible
        }
    }

    public struct NetworkSettings
    {
        public string Name { get; private set; }
    }

    public class ServerSettings
    {
        public string Uri { get; private set; }
        public string Description { get; private set; }
        public int Port { get; private set; }
        public bool Ssl { get; private set; }

        public ServerSettings(string uri, string description, int port, bool ssl)
        {
            Uri = uri;
            Description = description;
            Port = port;
            Ssl = ssl;
        }
    }
}