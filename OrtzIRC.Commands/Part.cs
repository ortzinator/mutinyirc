namespace OrtzIRC.Commands
{
    using System;
    using OrtzIRC.Common;
    using OrtzIRC.PluginFramework;

    //What if the author wants one plugin that adds many features?

    /// <summary>
    /// Parts a channel
    /// </summary>
    [Plugin] 
    public class Part : ICommand
    {
        private string defaultMessage = "Goodbye!";

        /// <summary>
        /// Parts the current channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public void Execute(Channel context, string message)
        {
            context.Part(message);
        }

        /// <summary>
        /// Parts the specified channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public void Execute(Channel context, ChannelInfo channel)
        {
            Execute(context.Server, channel, defaultMessage);
        }

        /// <summary>
        /// Parts the specified channel with the specified message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Execute(Channel context, ChannelInfo channel, string message)
        {
            Execute(context.Server, channel, message);
        }

        /// <summary>
        /// Parts the specified channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public void Execute(Server context, ChannelInfo channel)
        {
            Execute(context, channel, defaultMessage);
        }

        /// <summary>
        /// Parts the specified channel with the specified message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Execute(Server context, ChannelInfo channel, string message)
        {
            if (context.ChanManager.InChannel(channel.Name))
            {
                context.ChanManager.GetChannel(channel.Name).Part(message);
            }
            else
            {
                //TODO: Error?
            }
        }

        /// <summary>
        /// Parts the specified channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public void Execute(PrivateMessageSession context, ChannelInfo channel)
        {
            Execute(context.ParentServer, channel, defaultMessage);
        }

        /// <summary>
        /// Parts the specified channel with the specified message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Execute(PrivateMessageSession context, ChannelInfo channel, string message)
        {
            Execute(context.ParentServer, channel, message);
        }
    }
}