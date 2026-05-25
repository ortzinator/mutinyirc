namespace MutinyIRC.PluginFramework
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        public PluginAttribute() { }
        public PluginAttribute(string name)
        {
            Name = name;
        }
        public PluginAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
