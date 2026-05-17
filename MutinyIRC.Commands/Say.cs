namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// 
    /// </summary>
    [Plugin]
    public class Say : ICommand
    {
        /// <summary>
        /// Sends a message to the current channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Execute(Channel channel, string message)
        {
            channel.Say(message);
        }

        public void Execute(PrivateMessageSession pm, string message)
        {
            pm.Send(message);
        }
    }
}
