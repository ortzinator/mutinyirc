namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using OrtzIRC.Common;
    using OrtzIRC.Resources;

    public sealed class IrcSettingsManager
    {
        private static IrcSettingsManager instance;
        private static string settingsPath;

        public NetworkSettingsList Networks { get; private set; }

        private IrcSettingsManager()
        {
            Networks = new NetworkSettingsList();
        }

        public static IrcSettingsManager Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = new IrcSettingsManager();
                settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"OrtzIRC/servers.xml");
                instance.Load();
                return instance;
            }
        }

        public NetworkSettings AddNetwork(string networkName)
        {
            return Networks.AddNetwork(networkName);
        }

        public bool RemoveNetwork(NetworkSettings network)
        {
            return Networks.Remove(network);
        }

        public NetworkSettings GetNetwork(string name)
        {
            foreach (NetworkSettings network in Networks)
                if (network.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return network;

            return null;
        }

        public void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NetworkSettingsList), new XmlRootAttribute("EpicServerList"));
                var fs = new FileStream(settingsPath, FileMode.Create);
                TextWriter writer = new StreamWriter(fs, new System.Text.UTF8Encoding());
                serializer.Serialize(writer, Networks);
                writer.Close();
                fs.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Could not save IRC settings to disk", ChannelStrings.ErrorMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error); //hack
                //TODO: Log, or something, instead of MessageBox.
            }
        }

        private void Load()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NetworkSettingsList), new XmlRootAttribute("EpicServerList"));
                var fs = new FileStream(settingsPath, FileMode.Open);
                Networks = (NetworkSettingsList)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (FileNotFoundException)
            {
                NetworkSettings net = AddNetwork("RandomIRC");
                net.AddServer(new ServerSettings("apps.randomirc.com", "Random", "6667", false));
                net.AddChannel(new ChannelSettings("#ortzirc", true));
            }
        }

        public List<ServerSettings> GetAutoConnectServers()
        {
            var tmp = new List<ServerSettings>();
            foreach (var network in Networks)
            {
                foreach (var server in network.Servers)
                {
                    if (server.AutoConnect)
                    {
                        tmp.Add(server);
                    }
                }
            }
            return tmp;
        }

        public NetworkSettings GetNetwork(Server server)
        {
            foreach (NetworkSettings networkSettings in Networks)
            {
                foreach (ServerSettings serverSettings in networkSettings.Servers)
                {
                    if (server.Url == serverSettings.Url)
                    {
                        return networkSettings;
                    }
                }
            }
            return null;
        }

        public void DisableAutoConnect(Server server)
        {
            var settings = GetServer(server);
            settings.AutoConnect = false;
        }

        private ServerSettings GetServer(Server server)
        {
            foreach (NetworkSettings networkSettings in Networks)
            {
                foreach (ServerSettings serverSettings in networkSettings.Servers)
                {
                    if (server.Url == serverSettings.Url)
                    {
                        return serverSettings;
                    }
                }
            }
            return null;
        }
    }
}