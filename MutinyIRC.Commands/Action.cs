namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Sends a CTCP ACTION (the /me command) to a channel or private-message session.
    /// </summary>
    [Plugin("Me", "Sends a CTCP ACTION to the current channel or query. Usage: /me <action>")]
    public class Action : ICommand
    {
        /// <summary>
        /// Sends the action to the current channel.
        /// </summary>
        public void Execute(Channel channel, string message)
        {
            channel.Act(message);
        }

        /// <summary>
        /// Sends the action to the current private-message session.
        /// </summary>
        public void Execute(PrivateMessageSession pm, string message)
        {
            pm.SendAction(message);
        }
    }
}
