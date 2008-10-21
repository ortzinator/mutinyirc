namespace OrtzIRC.Plugins
{
    internal class CommandInfo
    {
        public string AssemblyPath { get; set; }
        public string ClassName { get; set; }

        public CommandInfo(string path, string name)
        {
            this.AssemblyPath = path;
            this.ClassName = name;
        }
    }
}
