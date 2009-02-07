namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;

    public class CommandInfo : PluginInfo
    {
        public string CommandName { get; private set; }

        public CommandInfo(string path, string name, Type type)
            : base(path, name, type)
        {
            CommandName = name;
        }
    }
}
