namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Opens a private-message tab for a nickname without sending anything.
    ///   Fails when the target is a server-side service (which never gets a tab).
    /// </summary>
    [Plugin("Query")]
    public class Query : ICommand
    {
        public CommandResultInfo Execute(Channel channel, string nick)
            => OpenQuery(channel.Server, nick);

        public CommandResultInfo Execute(Server server, string nick)
            => OpenQuery(server, nick);

        public CommandResultInfo Execute(PrivateMessageSession pm, string nick)
            => OpenQuery(pm.Server, nick);

        private static CommandResultInfo OpenQuery(Server server, string nick)
        {
            PrivateMessageSession session = server.GetOrCreatePM(nick);
            if (session == null)
                return CommandResultInfo.Fail($"Cannot open a query with the server service '{nick}'");

            return CommandResultInfo.Success(string.Empty);
        }
    }
}
