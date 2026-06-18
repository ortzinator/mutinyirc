namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// A command that sends private messages.
    /// </summary>
    [Plugin("Msg")]
    public class Message : ICommand
    {
        public CommandResultInfo Execute(MessageContext context, string user, string message)
            => SendPrivate(context.OwningServer, user, message);

        /// <summary>
        ///   Routes the outgoing message through a <see cref="PrivateMessageSession"/> so
        ///   the sender gets a tab + echoed line. When the target is a service nickname (no
        ///   session is created) it goes through <see cref="Server.MessageService"/>, which
        ///   echoes the line in the server window alongside the service's replies.
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
}
