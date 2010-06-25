namespace OrtzIRC.Common
{
    using System;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Xml;

    public class ChannelSettings : IXmlSerializable, IEquatable<ChannelSettings>
    {
        public ChannelSettings(string name, bool autojoin, string description, string key)
            : this(name, autojoin)
        {
            Description = description;
            Key = key;
        }

        public ChannelSettings(string name, bool autojoin)
        {
            Name = name;
            AutoJoin = autojoin;
        }

        public ChannelSettings() { }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public bool AutoJoin { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Description = reader.GetAttribute("Description") ?? String.Empty;
            Name = reader.GetAttribute("Name");
            Key = reader.GetAttribute("Key");
            AutoJoin = reader.GetAttribute("AutoJoin") == "True";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Description", Description);
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Key", Key);
            writer.WriteAttributeString("AutoJoin", AutoJoin.ToString());
        }

        public bool Equals(ChannelSettings other)
        {
            return other != null && other.Name == Name;
        }
    }
}
