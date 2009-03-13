namespace OrtzIRC.PluginFramework
{
    using System;

    public class CommandInfo : PluginInfo
    {
        public CommandInfo(string path, string fullName, string commandName, Type type)
            : base(path, fullName, type)
        {
            CommandName = commandName;
        }

        public string CommandName { get; private set; }
    }
}
