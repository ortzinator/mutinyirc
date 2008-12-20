namespace OrtzIRC.PluginFramework
{
    using System;

    public class PluginInfo
    {
        public string AssemblyPath { get; private set; }

        public string ClassName { get; private set; }

        /// <summary>
        /// The plugin interface it uses
        /// </summary>
        public Type Type { get; private set; }

        public PluginInfo(string path, string name, Type type)
        {
            AssemblyPath = path;
            ClassName = name;
            Type = type;
        }
    }
}
