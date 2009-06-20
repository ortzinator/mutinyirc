namespace OrtzIRC.Common
{
    public class ServerSettings
    {
        public ServerSettings(string uri, string description, string ports, bool ssl)
        {
            Uri = uri;
            Description = description;
            Ports = ports;
            Ssl = ssl;
        }

        public ServerSettings() { }

        public string Uri { get; set; }
        public string Description { get; set; }
        public string Ports { get; set; }
        public bool Ssl { get; set; }
        public int Id { get; set; }

        private int[] PortList
        {
            get
            {
                return PortsStringToArray(Ports);
            }
        }


        private int[] PortsStringToArray(string ports)
        {
            var bits = ports.Split(',');

            return new int[] {}; //TODO
        }
    }
}
