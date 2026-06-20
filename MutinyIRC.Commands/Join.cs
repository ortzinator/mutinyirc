namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Joins a channel
    /// </summary>
    [Plugin("Join", "Joins a channel. Usage: /join <#channel> [key]")]
    public class Join : ICommand
    {
        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(MessageContext context, ChannelInfo channelToJoin)
        {
            context.OwningServer.JoinChannel(channelToJoin.Name);
        }

        /// <summary>
        /// Joins you to the specified channel using the given key.
        /// </summary>
        public void Execute(MessageContext context, ChannelInfo channelToJoin, string key)
        {
            context.OwningServer.JoinChannel(channelToJoin.Name, key);
        }
    }
}
