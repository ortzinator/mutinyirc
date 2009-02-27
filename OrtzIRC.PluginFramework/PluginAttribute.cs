namespace OrtzIRC.PluginFramework
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

        public string Name { get; private set; }
    }
}
