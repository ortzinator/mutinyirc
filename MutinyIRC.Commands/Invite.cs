using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Invites a user to a channel (the /invite command). Used as
/// <c>/invite nickname #channel</c>, or as <c>/invite nickname</c> from a channel to
/// invite the user to that channel.
/// </summary>
[Plugin("Invite", "Invites a user to a channel. Usage: /invite <nick> [#channel]")]
public class Invite : ICommand
{
    /// <summary>
    /// Invites a user to the channel this command was run from.
    /// </summary>
    public CommandResultInfo Execute(Channel context, string nick)
        => SendInvite(context.Server, nick, context.Name);

    /// <summary>
    /// Invites a user to the named channel.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string nick, ChannelInfo channel)
        => SendInvite(context.OwningServer, nick, channel.Name);

    private static CommandResultInfo SendInvite(Server server, string nick, string channel)
    {
        server.Connection.Sender.Invite(nick, channel);
        return CommandResultInfo.Success(string.Empty);
    }
}
