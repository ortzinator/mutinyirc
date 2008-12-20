namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;

    public class CommandInfo : PluginInfo
    {
        public List<CommandAutocompleteAttribute> Autocompletes { get; private set; }

        public CommandInfo(string path, string name, Type type, List<CommandAutocompleteAttribute> autocompletes) 
            : base(path, name, type)
        {
            AssemblyPath = path;
            ClassName = name;
            Type = type;
            Autocompletes = autocompletes;
        }
    }
}
