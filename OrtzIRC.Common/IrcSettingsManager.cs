namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public sealed class IRCSettingsManager
    {
        private static IRCSettingsManager instance;
        private static SqlConnection db;

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
                    db = new SqlConnection("Data Source=ircsettings.db;Version=3;");
                    db.Open();
                }
                return instance;
            }
        }

        public bool AddNetwork(string networkName)
        {
            var cmd = new SqlCommand(string.Format("INSERT INTO networks (Name) VALUES ({0})", networkName), db);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<NetworkSettings> GetNetworks()
        {
            var set = new List<NetworkSettings>();
            var cmd = new SqlCommand("SELECT * FROM networks", db);

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var network = new NetworkSettings();
                network.Name = (string)rdr["Name"];
                set.Add(network);
            }
            return set;
        }

        public NetworkSettings GetNetwork(int id)
        {
            var cmd = new SqlCommand(string.Format("SELECT * FROM networks WHERE id = {0}", id), db);

            SqlDataReader rdr = cmd.ExecuteReader();

            rdr.Read();

            return new NetworkSettings {Name = (string)rdr["Name"]};
        }

    }
}