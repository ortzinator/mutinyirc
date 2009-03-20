namespace OrtzIRC.PluginFramework
{
    using System.Collections.Generic;
    using OrtzIRC.Common;

    public class CommandExecutionInfo
    {
        public string Name { get; set; }
        public List<object> ParameterList { get; set; }
        public MessageContext Context { get; set; }
    }
}
