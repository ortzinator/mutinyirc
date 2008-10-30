namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;

    [Command]
    public class Join
    {
        /// <summary>
        /// Joins a channel.
        /// </summary>
        /// <param name="channel">The channel to join.</param>
        public void Execute(Channel channel, string channelToJoin)
        {
            channel.Server.JoinChannel(channelToJoin);
        }

        public void Execute(Server server, string channelToJoin)
        {
            server.JoinChannel(channelToJoin);
        }

        public void Execute(PrivateMessageSession privMessage, string channelToJoin)
        {
            privMessage.ParentServer.JoinChannel(channelToJoin);
        }
    }
}
