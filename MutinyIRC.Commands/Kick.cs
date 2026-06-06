namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Kicks a user from the current channel (the /kick command).
    /// </summary>
    [Plugin]
    public class Kick : ICommand
    {
        /// <summary>
        /// Kicks a user from the channel, using the kicker's nick as the reason.
        /// </summary>
        public void Execute(Channel channel, string nick)
        {
            channel.Kick(nick, channel.Server.UserNick);
        }

        /// <summary>
        /// Kicks a user from the channel with the given reason.
        /// </summary>
        public void Execute(Channel channel, string nick, string reason)
        {
            channel.Kick(nick, reason);
        }
    }
}