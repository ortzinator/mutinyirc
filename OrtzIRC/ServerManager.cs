using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;

namespace OrtzIRC
{
    public class Network
    {
        private bool _changed = false;
        public bool Changed
        {
            get { return _changed; }
        }

        private int _id;
        public int ID
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _changed = true;
                }
                _id = value;
            }
        }

        private int _name;
        public int Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _changed = true;
                }
                _name = value;
            }
        }

        private List<Server> _servers;
        public List<Server> ServerCollection
        {
            get { return _servers; }
        }

        public void AddServer(Server server)
        {
            _servers.Add(server);
            _changed = true;
        }

    }

    public class Server
    {
        private bool _changed = false;

        private int _id;

        private string _URI;
        public string URI
        {
            get { return _URI; }
            set
            {
                if (value != _URI)
                {
                    _changed = true;
                }
                _URI = value; 
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _changed = true;
                }
                _description = value;
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _changed = true;
                }
                _port = value;
            }
        }

        public Server(string URI, string description, int port, Network network)
        {
            this._URI = URI;
            this._description = description;
            this._port = port;
            network.AddServer(this);
        }
    }

    public class ServerManager
    {
        private static ServerManager _instance;

        protected ServerManager()
        {
            LoadServers();
        }

        public static ServerManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ServerManager();
            }
            return _instance;
        }

        public static void AddServer(Server server)
        {

        }
        public static void AddServer(string URI, string description, int port, int networkID)
        {

        }

        private void LoadServers()
        {
            DataSet set = new DataSet();

            SQLiteConnection db = new SQLiteConnection("Data Source=F:\\settings.s3db");
            db.Open();

            SQLiteDataAdapter networks = new SQLiteDataAdapter(new SQLiteCommand("SELECT * FROM networks", db));
            networks.FillSchema(set, SchemaType.Source);
            networks.Fill(set);
            DataTable pTable = set.Tables["Table"];
            pTable.TableName = "Networks";


            SQLiteDataAdapter servers = new SQLiteDataAdapter(new SQLiteCommand("SELECT * FROM servers", db));
            servers.FillSchema(set, SchemaType.Source);
            servers.Fill(set);
            pTable = set.Tables["Table"];
            pTable.TableName = "Servers";

            set.Relations.Add(new DataRelation("ParentChild",
                set.Tables["Networks"].Columns["id"],
                set.Tables["Servers"].Columns["network_id"]));

            foreach (DataTable dt in set.Tables)
            {
                PrintTable(dt);
            }

            db.Close();

        }

        private void PrintTable(DataTable dt)
        {
            Console.WriteLine("\n***** Rows in DataTable *****");
            
            DataTableReader dtReader = dt.CreateDataReader();
            
            while (dtReader.Read())
            {
                for (int i = 0; i < dtReader.FieldCount; i++)
                {
                    Console.Write("{0} = {1} ",
                    dtReader.GetName(i),
                    dtReader.GetValue(i).ToString().Trim());
                }
                Console.WriteLine();
            }
            dtReader.Close();
        }
    }
}
