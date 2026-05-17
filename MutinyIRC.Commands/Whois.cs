namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    [Plugin]
    public class Whois : ICommand
    {
        public void Execute(Channel context, string nick)
        {
            context.Server.Connection.Sender.Whois(nick);
        }

        public void Execute(Server context, string nick)
        {
            context.Connection.Sender.Whois(nick);
        }

        public void Execute(PrivateMessageSession context, string nick)
        {
            context.Server.Connection.Sender.Whois(nick);
        }
    }
}