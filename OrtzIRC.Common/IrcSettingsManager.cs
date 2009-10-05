using System.Windows.Forms;

namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    public sealed class IRCSettingsManager
    {
        private static IRCSettingsManager instance;

        public NetworkSettingsList Networks { get; private set; }

        private IRCSettingsManager()
        {
            Networks = new NetworkSettingsList();
        }

        public static IRCSettingsManager Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = new IRCSettingsManager();
                instance.Load();
                return instance;
            }
        }

        public NetworkSettings AddNetwork(string networkName)
        {
            return Networks.AddNetwork(networkName);
        }

        public NetworkSettings GetNetwork(string name)
        {
            foreach (var network in Networks)
                if (network.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return network;

            return null;
        }

        public void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NetworkSettingsList), new XmlRootAttribute("EpicServerList"));
                var fs = new FileStream("servers.xml", FileMode.Create); //TODO: App setting
                TextWriter writer = new StreamWriter(fs, new System.Text.UTF8Encoding());
                serializer.Serialize(writer, Networks);
                writer.Close();
                fs.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Could not save IRC settings to disk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //TODO: Log, or something.
            }
        }

        private void Load()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NetworkSettingsList), new XmlRootAttribute("EpicServerList"));
                var fs = new FileStream("servers.xml", FileMode.Open); //TODO: App setting
                Networks = (NetworkSettingsList)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}