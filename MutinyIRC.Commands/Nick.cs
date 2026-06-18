namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Parts a channel
    /// </summary>
    [Plugin]
    public class Nick : ICommand
    {
        /// <summary>
        /// Changes your nick
        /// </summary>
        /// <param name="context"></param>
        /// <param name="nick"></param>
        public void Execute(MessageContext context, string nick)
        {
            context.OwningServer.ChangeNick(nick);
        }
    }
}
