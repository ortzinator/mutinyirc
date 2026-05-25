namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Sends a CTCP ACTION (the /me command) to a channel or private-message session.
    /// </summary>
    [Plugin("Me")]
    public class Action : ICommand
    {
        public void Execute(Channel channel, string message)
        {
            channel.Act(message);
        }

        public void Execute(PrivateMessageSession pm, string message)
        {
            pm.SendAction(message);
        }
    }
}
