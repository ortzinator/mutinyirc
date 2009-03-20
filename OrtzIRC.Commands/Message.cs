namespace OrtzIRC.Commands
{
    using Common;
    using PluginFramework;

    /// <summary>
    /// A command that sends private messages.
    /// </summary>
    [Plugin("Msg")]
    public class Message : ICommand
    {
        /// <summary>
        /// Sends a private message to the specified user.
        /// </summary>
        /// <param name="channel">The context where the command is executed from.</param>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public CommandResultInfo Execute(Channel channel, string user, string message)
        {
            // todo - implement message command; MessageContext needs to have a way of getting at the Server (a abstract Server property would work)

            channel.Server.MessageUser(user, message);
            return new CommandResultInfo { Message = "", Result = CommandResult.Success };
        }
    }
}
