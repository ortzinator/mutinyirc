namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Sets or clears the local user's away status (the /away command). With a message,
    ///   <c>/away &lt;message&gt;</c> marks you away; bare <c>/away</c> clears it. The server's
    ///   confirmation (RPL_NOWAWAY / RPL_UNAWAY) is what actually flips
    ///   <see cref="Server.IsAway"/>, so nothing is echoed here.
    /// </summary>
    /// <remarks>
    ///   Works from any window; the command is always routed to that window's server. The tail
    ///   is taken raw so the away message keeps its spacing. Clearing has its own parameterless
    ///   overloads because the dispatcher only matches a <see cref="RawArgumentsAttribute"/>
    ///   overload when at least one token is present, and the underlying AWAY command rejects an
    ///   empty message anyway.
    /// </remarks>
    [Plugin]
    public class Away : ICommand
    {
        /// <summary>Marks away with a message from any window.</summary>
        [RawArguments]
        public CommandResultInfo Execute(MessageContext context, string message)
            => SetAway(context.OwningServer, message);

        /// <summary>Clears away (bare /away) from any window.</summary>
        public CommandResultInfo Execute(MessageContext context) => ClearAway(context.OwningServer);

        private static CommandResultInfo SetAway(Server server, string message)
        {
            // A whitespace-only tail is still a single token, so it reaches this overload rather
            // than the parameterless one — treat it as a request to clear.
            if (string.IsNullOrWhiteSpace(message))
                return ClearAway(server);

            server.SetAway(message);
            return CommandResultInfo.Success(string.Empty);
        }

        private static CommandResultInfo ClearAway(Server server)
        {
            server.ClearAway();
            return CommandResultInfo.Success(string.Empty);
        }
    }
}
