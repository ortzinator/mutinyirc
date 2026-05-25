namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    /// Joins a channel
    /// </summary>
    [Plugin]
    public class Join : ICommand
    {
        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(Channel context, ChannelInfo channelToJoin)
        {
            Execute(context.Server, channelToJoin);
        }

        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(Server context, ChannelInfo channelToJoin)
        {
            context.JoinChannel(channelToJoin.Name);
        }

        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(PrivateMessageSession context, ChannelInfo channelToJoin)
        {
            Execute(context.Server, channelToJoin);
        }

        /// <summary>
        /// Joins you to the specified channel using the given key.
        /// </summary>
        public void Execute(Channel context, ChannelInfo channelToJoin, string key)
        {
            Execute(context.Server, channelToJoin, key);
        }

        /// <summary>
        /// Joins you to the specified channel using the given key.
        /// </summary>
        public void Execute(Server context, ChannelInfo channelToJoin, string key)
        {
            context.JoinChannel(channelToJoin.Name, key);
        }

        /// <summary>
        /// Joins you to the specified channel using the given key.
        /// </summary>
        public void Execute(PrivateMessageSession context, ChannelInfo channelToJoin, string key)
        {
            Execute(context.Server, channelToJoin, key);
        }
    }
}
