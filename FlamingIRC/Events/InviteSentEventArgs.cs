namespace FlamingIRC
{
    using System;

    public class InviteSentEventArgs : EventArgs
    {
        public string Nick;
        public string Channel;

        /// <summary>
        /// An Invite message was successfully sent to another user. 
        /// </summary>
        /// <param name="nick">The nick of the user who was invited</param>
        /// <param name="channel">The name of the channel the user was invited to join</param>
        /// <seealso cref="Listener.OnInviteSent"/>
        public InviteSentEventArgs(string nick, string channel)
        {
            Nick = nick;
            Channel = channel;
        }
    }
}