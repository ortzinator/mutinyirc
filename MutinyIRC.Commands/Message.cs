using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// A command that sends private messages.
/// </summary>
[Plugin("Msg", "Sends a private message to a user or channel. Usage: /msg <target> <message>")]
public class Message : ICommand
{
    /// <summary>
    /// Sends a private message to the given user or channel from any window.
    /// </summary>
    public CommandResultInfo Execute(MessageContext context, string user, string message)
        => SendPrivate(context.OwningServer, user, message);

    /// <summary>
    /// Routes the outgoing message through a <see cref="PrivateMessageSession"/> so
    /// the sender gets a tab + echoed line. When the target is a service nickname (no
    /// session is created) it goes through <see cref="Server.MessageService"/>, which
    /// echoes the line in the server window alongside the service's replies.
    /// </summary>
    private static CommandResultInfo SendPrivate(Server server, string user, string message)
    {
        PrivateMessageSession session = server.GetOrCreatePM(user);
        if (session != null)
            session.Send(message);
        else
            server.MessageService(user, message);

        return CommandResultInfo.Success(string.Empty);
    }
}
