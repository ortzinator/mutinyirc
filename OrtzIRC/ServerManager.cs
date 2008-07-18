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
        private DataSet _serverSet;

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

        public static void Add()
        {

        }

        private void LoadServers()
        {
            SQLiteConnection db = new SQLiteConnection("Data Source=F:\\settings.s3db");
            db.Open();

            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM networks");
            cmd.Connection = db;

            SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            
            while (reader.Read())
                Console.WriteLine("-> {0}, {1}", reader["id"], reader["name"]);
            reader.Close();

        }
    }

    public class WindowManager
    {
        private List<ServerForm> forms;
        private Form parentForm;

        public WindowManager(Form parentForm)
        {
            this.forms = new List<ServerForm>();
            this.parentForm = parentForm;
        }

        public void AddNewServer(string uri, string description, int port)
        {
            bool exists = false;

            // Check if window with this name exists
            for (int x = 0; x < parentForm.MdiChildren.Length; x++)
            {
                Form tempChild = (Form)parentForm.MdiChildren[x];
                if (tempChild.Text == uri)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                ServerForm newForm = new ServerForm();
                newForm.MdiParent = parentForm;
                newForm.Text = uri;
                newForm.Show();
            }
        }
    }
}
