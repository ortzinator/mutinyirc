namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    [Plugin]
    public class Whois : ICommand
    {
        public void Execute(MessageContext context, string nick)
        {
            context.OwningServer.Connection.Sender.Whois(nick);
        }
    }
}