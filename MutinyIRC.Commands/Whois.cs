namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Requests WHOIS information about a nick (the /whois command).
    /// </summary>
    [Plugin("Whois", "Requests WHOIS information about a nick. Usage: /whois <nick>")]
    public class Whois : ICommand
    {
        /// <summary>
        /// Requests WHOIS information for the given nick from any window.
        /// </summary>
        public void Execute(MessageContext context, string nick)
        {
            context.OwningServer.Connection.Sender.Whois(nick);
        }
    }
}