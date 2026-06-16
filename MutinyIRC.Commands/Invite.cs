namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Invites a user to a channel (the /invite command). Used as
    ///   <c>/invite nickname #channel</c>, or as <c>/invite nickname</c> from a channel to
    ///   invite the user to that channel.
    /// </summary>
    [Plugin("Invite")]
    public class Invite : ICommand
    {
        /// <summary>
        /// Invites a user to the channel this command was run from.
        /// </summary>
        public CommandResultInfo Execute(Channel context, string nick)
            => SendInvite(context.Server, nick, context.Name);

        /// <summary>
        /// Invites a user to the named channel.
        /// </summary>
        public CommandResultInfo Execute(Channel context, string nick, ChannelInfo channel)
            => SendInvite(context.Server, nick, channel.Name);

        /// <summary>
        /// Invites a user to the named channel.
        /// </summary>
        public CommandResultInfo Execute(Server context, string nick, ChannelInfo channel)
            => SendInvite(context, nick, channel.Name);

        /// <summary>
        /// Invites a user to the named channel.
        /// </summary>
        public CommandResultInfo Execute(PrivateMessageSession context, string nick, ChannelInfo channel)
            => SendInvite(context.Server, nick, channel.Name);

        private static CommandResultInfo SendInvite(Server server, string nick, string channel)
        {
            server.Connection.Sender.Invite(nick, channel);
            return CommandResultInfo.Success(string.Empty);
        }
    }
}