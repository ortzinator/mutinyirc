namespace MutinyIRC.PluginFramework
{
    using System;

    public class CommandInfo : PluginInfo
    {
        public CommandInfo(string path, string fullName, string commandName, Type type,
                           string description = null)
            : base(path, fullName, type)
        {
            CommandName = commandName;
            Description = description;
        }

        public string CommandName { get; private set; }

        public string Description { get; private set; }
    }
}
