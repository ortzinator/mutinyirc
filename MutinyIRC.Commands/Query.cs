using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Opens a private-message tab for a nickname, optionally sending an initial message.
/// Fails when the target is a server-side service (which never gets a tab).
/// </summary>
[Plugin("Query", "Opens a private-message tab for a nick, optionally sending a message. Usage: /query <nick> [message]")]
public class Query : ICommand
{
    /// <summary>
    /// Opens a private-message tab for the given nick without sending anything.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string nick)
        => OpenQuery(context.OwningServer, nick, null);

    /// <summary>
    /// Opens a private-message tab for the given nick and sends an initial message.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string nick, string message)
        => OpenQuery(context.OwningServer, nick, message);

    private static CommandResultInfo OpenQuery(Server server, string nick, string message)
    {
        PrivateMessageSession session = server.GetOrCreatePM(nick);
        if (session == null)
            return CommandResultInfo.Fail($"Cannot open a query with the server service '{nick}'");

        if (!string.IsNullOrEmpty(message))
            session.Send(message);

        return CommandResultInfo.Success(string.Empty);
    }
}
