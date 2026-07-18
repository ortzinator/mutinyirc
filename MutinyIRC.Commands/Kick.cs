using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Kicks a user from the current channel (the /kick command).
/// </summary>
[Plugin("Kick", "Kicks a user from the current channel. Usage: /kick <nick> [reason]")]
public class Kick : ICommand
{
    /// <summary>
    /// Kicks a user from the channel, using the kicker's nick as the reason.
    /// </summary>
    public void Execute(Channel channel, string nick)
    {
        channel.Kick(nick, channel.Server.UserNick);
    }

    /// <summary>
    /// Kicks a user from the channel with the given reason.
    /// </summary>
    public void Execute(Channel channel, string nick, string reason)
    {
        channel.Kick(nick, reason);
    }
}
