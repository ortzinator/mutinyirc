using OrtzIRC.Common;

namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    public class NetworkSettings : IXmlSerializable
    {
        public NetworkSettings(string name)
        {
            Name = name;
            Servers = new List<ServerSettings>();
        }

        public NetworkSettings() { }

        public string Name { get; set; }
        public List<ServerSettings> Servers { get; private set; }
        public List<ChannelSettings> Channels { get; set; }

        public ServerSettings GetRandomServer()
        {
            var rand = new Random();
            return Servers[rand.Next(Servers.Count - 1)];
        }

        public void AddServer(ServerSettings server)
        {
            if (Servers.Contains(server))
                return;

            server.Network = this;
            Servers.Add(server);
        }

        public ServerSettings AddServer()
        {
            var server = new ServerSettings();
            AddServer(server);
            return server;
        }

        public override string ToString()
        {
            return Name;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("Name");
            reader.Read();

            Servers = new List<ServerSettings>();

            while (reader.IsEmptyElement)
            {
                var net = new ServerSettings();
                net.ReadXml(reader);
                AddServer(net);
                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);

            foreach (ServerSettings server in Servers)
            {
                writer.WriteStartElement("Server");
                server.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public bool RemoveServer(ServerSettings serverSettings)
        {
            return Servers.Remove(serverSettings);
        }
    }
}
