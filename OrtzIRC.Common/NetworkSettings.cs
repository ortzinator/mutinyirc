using System;
using System.Collections.Generic;
using System.Xml;

namespace OrtzIRC.Common
{
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

        public ServerSettings GetRandomServer()
        {
            var rand = new Random();
            return Servers[rand.Next(Servers.Count)];
        }

        public void AddServer(ServerSettings server)
        {
            if (Servers.Contains(server))
                return;

            Servers.Add(server);
        }

        public ServerSettings AddServer()
        {
            var server = new ServerSettings();
            Servers.Add(server);
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
                Servers.Add(net);
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
