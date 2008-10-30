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
        private string _name;
    }

    public struct ServerSettings
    {
        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private bool _ssl;
        public bool Ssl
        {
            get { return _ssl; }
            set { _ssl = value; }
        }

        public ServerSettings(string uri, string description, int port, bool ssl)
        {
            _uri = uri;
            _description = description;
            _port = port;
            _ssl = ssl;
        }
    }
}