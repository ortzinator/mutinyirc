namespace OrtzIRC.Plugins
{
    internal class PluginInfo
    {
        public string AssemblyPath { get; set; }
        public string ClassName { get; set; }

        public PluginInfo(string path, string name)
        {
            this.AssemblyPath = path;
            this.ClassName = name;
        }
    }
}
