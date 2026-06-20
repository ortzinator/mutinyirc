namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Sends a line straight to the server, unparsed (the /raw command). Whatever follows
    ///   <c>/raw</c> is forwarded verbatim, so the caller must supply a valid IRC command
    ///   (e.g. <c>/raw WHOIS someone</c>, <c>/raw PRIVMSG #chan :hi</c>).
    /// </summary>
    /// <remarks>
    ///   Works from any window; the command is always routed to that window's server. The tail
    ///   is taken raw so spaces, colons, and a leading <c>+</c>/<c>-</c> are preserved.
    /// </remarks>
    [Plugin("Raw", "Sends a line straight to the server, unparsed. Usage: /raw <command>")]
    public class Raw : ICommand
    {
        /// <summary>
        /// Forwards the raw command line to the current window's server.
        /// </summary>
        [RawArguments]
        public CommandResultInfo Execute(MessageContext context, string command)
            => Send(context.OwningServer, command);

        private static CommandResultInfo Send(Server server, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return CommandResultInfo.Fail("Usage: /raw <command>");

            server.SendRaw(command);
            return CommandResultInfo.Success(string.Empty);
        }
    }
}
