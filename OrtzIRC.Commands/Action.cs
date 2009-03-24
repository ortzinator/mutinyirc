namespace OrtzIRC.Commands
{
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    /// <summary>
    /// Parts a channel
    /// </summary>
    [Plugin("Me")]
    public class Action : ICommand
    {
        /// <summary>
        /// Sends a message to the current channel as an action
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Execute(Channel channel, string message)
        {
            channel.Act(message);
        }
    }
}
