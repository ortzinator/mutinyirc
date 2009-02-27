namespace OrtzIRC.PluginFramework
{
    using System;

    public class CommandInfo : PluginInfo
    {
        public CommandInfo(string path, string name, Type type)
            : base(path, name, type)
        {
            CommandName = name;
        }

        public string CommandName { get; private set; }
    }
}
