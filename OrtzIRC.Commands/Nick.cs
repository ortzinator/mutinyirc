namespace OrtzIRC.Commands
{
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    /// <summary>
    /// Parts a channel
    /// </summary>
    [Plugin]
    public class Nick : ICommand
    {
        /// <summary>
        /// Changes your nick
        /// </summary>
        /// <param name="server"></param>
        /// <param name="nick"></param>
        public void Execute(Server server, string nick)
        {
            server.ChangeNick(nick);
        }

        /// <summary>
        /// Changes your nick
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        public void Execute(Channel channel, string nick)
        {
            Execute(channel.Server, nick);
        }
    }
}
