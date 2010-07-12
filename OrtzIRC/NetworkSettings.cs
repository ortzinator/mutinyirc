using OrtzIRC.Common;

namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    public class NetworkSettings : IXmlSerializable, IEquatable<NetworkSettings>
    {
        public NetworkSettings(string name)
        {
            Name = name;
            Servers = new List<ServerSettings>();
            Channels = new List<ChannelSettings>();
        }

        public NetworkSettings() : this("") { }

        public string Name { get; set; }
        public List<ServerSettings> Servers { get; private set; }
        public List<ChannelSettings> Channels { get; private set; }

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

        public bool Equals(NetworkSettings other)
        {
            return other != null && other.Name == Name;
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
                if (reader.Name == "Server")
                {
                    var net = new ServerSettings();
                    net.ReadXml(reader);
                    AddServer(net);
                    reader.Read();
                }
                else if (reader.Name == "Channel")
                {
                    var chan = new ChannelSettings();
                    chan.ReadXml(reader);
                    AddChannel(chan);
                    reader.Read();
                }
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

            foreach (ChannelSettings channel in Channels)
            {
                writer.WriteStartElement("Channel");
                channel.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public bool RemoveServer(ServerSettings serverSettings)
        {
            return Servers.Remove(serverSettings);
        }

        public ServerSettings GetServer(string url)
        {
            foreach (ServerSettings server in Servers)
            {
                if (server.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase))
                {
                    return server;
                }
            }
            return null;
        }

        public ChannelSettings AddChannel(Channel data)
        {
            var chan = new ChannelSettings();
            chan.Name = data.Name;
            AddChannel(chan);
            return chan;
        }

        public void AddChannel(ChannelSettings channel)
        {
            if (Channels.Contains(channel))
                return; //todo ???

            Channels.Add(channel);
        }

        public ChannelSettings GetChannel(string name)
        {
            foreach (ChannelSettings channel in Channels)
            {
                if (channel.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return channel;
            }
            return null;
        }
    }
}
