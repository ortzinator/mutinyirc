namespace FlamingIRC
{
    using System;

    public class InviteEventArgs : EventArgs
    {
        /// <summary>
        /// The nick of the user who was invited
        /// </summary>
        public string Nick;

        /// <summary>
        /// The name of the channel the user was invited to join
        /// </summary>
        public string Channel;

        /// <summary>
        /// An invite was either sent or recieved
        /// </summary>
        /// <param name="nick">The nick of the user who was invited or sent the invite</param>
        /// <param name="channel">The name of the channel the user was invited to join</param>
        /// <seealso cref="Listener.OnInviteSent"/>
        public InviteEventArgs(string nick, string channel)
        {
            Nick = nick;
            Channel = channel;
        }
    }
}