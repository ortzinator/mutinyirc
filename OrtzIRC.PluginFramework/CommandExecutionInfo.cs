namespace OrtzIRC.PluginFramework
{
    using System.Collections.Generic;

    public class CommandExecutionInfo
    {
        public string Name { get; set; }
        public List<string> ParameterList { get; set; }
    }
}
