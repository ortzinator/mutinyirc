using System.Collections.Generic;
using MutinyIRC.Common;

namespace MutinyIRC.PluginFramework;

public class CommandExecutionInfo
{
    public string Name { get; set; }
    public List<object> ParameterList { get; set; }
    public MessageContext Context { get; set; }
}
