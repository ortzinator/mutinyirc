namespace OrtzIRC
{
    internal class Plugin
    {
        public string AssemblyPath { get; set; }
        public string ClassName { get; set; }

        public Plugin(string path, string name)
        {
            this.AssemblyPath = path;
            this.ClassName = name;
        }
    }
}
