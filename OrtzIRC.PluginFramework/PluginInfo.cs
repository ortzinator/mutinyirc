namespace OrtzIRC.PluginFramework
{
    using System;

    public class PluginInfo
    {
        private Type _type;

        public PluginInfo(string path, string fullName, Type type)
        {
            AssemblyPath = path;
            FullName = fullName;
            Type = type;
        }

        public string AssemblyPath { get; protected set; }

        public string FullName { get; protected set; }

        /// <summary>
        /// The plugin interface it uses
        /// </summary>
        public Type Type
        {
            get { return _type; }
            protected set
            {
                var pluginType = typeof(IPlugin);
                if (!pluginType.IsAssignableFrom(value))
                    throw new ArgumentException("Type is not a Plugin");

                _type = value;
            }
        }
    }
}
