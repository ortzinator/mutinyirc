namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Sends a NOTICE to a user or channel (the /notice command). Unlike /msg, no private
    ///   message tab is opened — the sent notice is echoed in the server window.
    /// </summary>
    /// <remarks>
    ///   Syntax is <c>/notice &lt;target&gt; &lt;message&gt;</c>. The first token is the target
    ///   (a nick or channel); everything after it is the message text, passed through verbatim.
    /// </remarks>
    [Plugin]
    public class Notice : ICommand
    {
        /// <summary>Sends a notice from any window, echoing it in that window.</summary>
        [RawArguments]
        public CommandResultInfo Execute(MessageContext context, string rest)
            => SendNotice(context, context.OwningServer, rest);

        /// <summary>
        ///   Sends the notice over <paramref name="server"/> and echoes it in
        ///   <paramref name="context"/> — the window the command was issued from.
        /// </summary>
        private static CommandResultInfo SendNotice(MessageContext context, Server server, string rest)
        {
            int sep = rest.IndexOf(' ');
            string target = sep < 0 ? rest : rest.Substring(0, sep);
            string message = sep < 0 ? string.Empty : rest.Substring(sep + 1);

            if (target.Length == 0 || message.Length == 0)
                return CommandResultInfo.Fail("Usage: /notice <target> <message>");

            server.SendNotice(target, message);
            context.OnNoticeSent(target, message);
            return CommandResultInfo.Success(string.Empty);
        }
    }
}
