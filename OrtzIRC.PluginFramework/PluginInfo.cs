namespace OrtzIRC.PluginFramework
{
    using System;

    public class PluginInfo
    {
        public string AssemblyPath { get; protected set; }

        public string ClassName { get; protected set; }

        /// <summary>
        /// The plugin interface it uses
        /// </summary>
        public Type Type { get; protected set; }

        public PluginInfo(string path, string name, Type type)
        {
            AssemblyPath = path;
            ClassName = name;
            Type = type;
        }
    }
}
