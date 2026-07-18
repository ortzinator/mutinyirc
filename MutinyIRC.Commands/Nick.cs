using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Parts a channel
/// </summary>
[Plugin("Nick", "Changes your nickname. Usage: /nick <newnick>")]
public class Nick : ICommand
{
    /// <summary>
    /// Changes your nick
    /// </summary>
    /// <param name="context"></param>
    /// <param name="nick"></param>
    public void Execute(MessageContext context, string nick)
    {
        context.OwningServer.ChangeNick(nick);
    }
}
