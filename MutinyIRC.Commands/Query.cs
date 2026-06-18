namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Opens a private-message tab for a nickname, optionally sending an initial message.
    ///   Fails when the target is a server-side service (which never gets a tab).
    /// </summary>
    [Plugin("Query")]
    public class Query : ICommand
    {
        public CommandResultInfo Execute(MessageContext context, string nick)
            => OpenQuery(context.OwningServer, nick, null);

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
}
