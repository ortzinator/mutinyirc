namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    public class Join : ICommand
    {
        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(Channel context, ChannelInfo channelToJoin)
        {
            this.Execute(context.Server, channelToJoin);
        }

        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(Server context, ChannelInfo channelToJoin)
        {
            context.JoinChannel(channelToJoin);
        }

        /// <summary>
        /// Joins you to the specified channel.
        /// </summary>
        public void Execute(PrivateMessageSession context, ChannelInfo channelToJoin)
        {
            this.Execute(context.ParentServer, channelToJoin);
        }
    }
}
