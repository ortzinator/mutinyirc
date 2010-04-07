namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;


    public class NetworkSettingsList : List<NetworkSettings>, IXmlSerializable
    {
        public NetworkSettings AddNetwork(string name)
        {
            var net = new NetworkSettings(name);

            if (Contains(net))
                return null;

            Add(net);
            return net;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            Clear();

            reader.Read();
            while (reader.IsStartElement("Network"))
            {
                var net = new NetworkSettings();
                net.ReadXml(reader);
                Add(net);
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var settings in this)
            {
                writer.WriteStartElement("Network");
                settings.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
