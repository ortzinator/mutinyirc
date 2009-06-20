namespace OrtzIRC.Common
{
    using System.Collections.Generic;
    using System.Data.SQLite;

    public sealed class IRCSettingsManager
    {
        private static IRCSettingsManager instance;
        private static SQLiteConnection db;

        private IRCSettingsManager() { } //this is just here to make the class inconstructible

        public static IRCSettingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IRCSettingsManager();

                    db = new SQLiteConnection("Data Source=ircsettings.db;");
                    db.Open();

                    CheckDatabase();
                }
                return instance;
            }
        }

        private static void CheckDatabase()
        {
            var cmd = db.CreateCommand();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS networks (
                                id integer PRIMARY KEY AUTOINCREMENT NOT NULL, 
                                name varchar(50) UNIQUE COLLATE NOCASE NOT NULL)";

            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS servers (
                                id integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                                description varchar(50) NOT NULL,
                                uri varchar(50) NOT NULL,
                                ports varchar(50),
                                network_id integer NOT NULL,
                                CONSTRAINT fk_servers FOREIGN KEY (network_id) REFERENCES networks (id))";

            cmd.ExecuteNonQuery();
            //TODO: Better way to execute multiple queries?
        }

        public bool AddNetwork(string networkName)
        {
            var cmd = db.CreateCommand();

            cmd.CommandText = "INSERT INTO networks (name) VALUES ('@NetworkName')"; //TODO: Sanitize?
            cmd.Parameters.AddWithValue("@NetworkName", networkName);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<NetworkSettings> GetNetworks()
        {
            var set = new List<NetworkSettings>();
            var cmd = db.CreateCommand(); 
            cmd.CommandText = "SELECT * FROM networks";

            SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var network = new NetworkSettings();
                network.Name = (string)rdr["name"];
                network.Id = rdr.GetInt32(0);
                set.Add(network);
            }
            return set;
        }

        public NetworkSettings GetNetwork(int id)
        {
            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM networks WHERE id = @Id";
            cmd.Parameters.AddWithValue("@Id", id);

            SQLiteDataReader rdr = cmd.ExecuteReader();

            rdr.Read();

            return new NetworkSettings { Name = (string)rdr["name"], Id = (int)rdr["id"] };
        }

        public List<ServerSettings> GetServers(int id)
        {
            var set = new List<ServerSettings>();
            var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM servers WHERE network_id = @NetworkId";
            cmd.Parameters.AddWithValue("@NetworkId", id);

            SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var server = new ServerSettings();
                server.Description = (string)rdr["description"];
                server.Uri = (string)rdr["uri"];
                server.Ports = (string)rdr["ports"];
                //server.Ssl = (bool)rdr["ssl"];
                server.Id = rdr.GetInt32(0);
                set.Add(server);
            }
            return set;
        }
    }
}