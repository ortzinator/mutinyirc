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
                if (!value.IsSubclassOf(typeof(IPlugin)))
                    throw new ArgumentException();

                _type = value;
            }
        }
    }
}
