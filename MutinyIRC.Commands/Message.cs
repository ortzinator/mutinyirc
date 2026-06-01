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
        public CommandResultInfo Execute(Channel channel, string user, string message)
            => SendPrivate(channel.Server, user, message);

        public CommandResultInfo Execute(Server server, string user, string message)
            => SendPrivate(server, user, message);

        public CommandResultInfo Execute(PrivateMessageSession pm, string user, string message)
            => SendPrivate(pm.Server, user, message);

        /// <summary>
        ///   Routes the outgoing message through a <see cref="PrivateMessageSession"/> so
        ///   the sender gets a tab + echoed line. Falls back to the raw wire send when the
        ///   target is a service nickname (no session is created in that case).
        /// </summary>
        private static CommandResultInfo SendPrivate(Server server, string user, string message)
        {
            PrivateMessageSession session = server.GetOrCreatePM(user);
            if (session != null)
                session.Send(message);
            else
                server.MessageUser(user, message);

            return CommandResultInfo.Success(string.Empty);
        }
    }
}
