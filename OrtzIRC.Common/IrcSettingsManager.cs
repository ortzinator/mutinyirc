namespace OrtzIRC.Common
{
    using System.Data.Linq;
    using System.Data.Objects;

    public sealed class IRCSettingsManager
    {
        private static IRCSettingsManager instance;
        private static serversEntities db;

        private IRCSettingsManager()
        {
            //this is just here to make the class inconstructible
        }

        public static IRCSettingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IRCSettingsManager();
                    db = new serversEntities();
                }
                return instance;
            }
        }

        public void AddNetwork(string networkName)
        {
            Networks net = new Networks {Name = networkName};
            db.AddToNetworks(net);
            db.SaveChanges();
        }

        public void GetNetworks()
        {
            //var nets = from net in db.Networks where net.

        }

    }
}