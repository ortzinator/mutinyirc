namespace OrtzIRC.PluginFramework
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }

        public PluginAttribute(string name, string author, string version, string description)
        {
            Name = name;
            Author = author;
            Version = version;
            Description = description;
        }
    }
}
