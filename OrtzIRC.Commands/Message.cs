namespace OrtzIRC.Commands
{
    using Common;
    using PluginFramework;

    /// <summary>
    /// A command that sends private messages.
    /// </summary>
    [Plugin("Msg")]
    public class Message
    {
        /// <summary>
        /// Sends a private message to the specified user.
        /// </summary>
        /// <param name="context">The context where the command is executed from.</param>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public void Execute(MessageContext context, UserInfo user, string message)
        {
            // todo - implement message command; MessageContext needs to have a Send method (it can be abstract if necessary)
        }
    }
}
