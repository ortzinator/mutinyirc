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
    public class ServerSettingsManager
    {
        private static ServerSettingsManager _instance;

        protected ServerSettingsManager()
        {
            LoadServers();
        }

        public static ServerSettingsManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ServerSettingsManager();
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
