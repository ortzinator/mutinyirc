namespace FlamingIRC
{
    using System;

    public class NamesEventArgs : EventArgs
    {
        /// <summary>
        /// The channel the user is on. "@" is used for secret channels, "*" for private
        /// channels, and "=" for public channels.
        /// </summary>
        public string Channel;

        /// <summary>
        /// A list of nicks on the channel. If this is the last reply
        /// then it will be empty. Nicks prefixed with a '@' are channel
        /// operators. Nicks prefixed with a '+' have voice privileges on
        /// a moderated channel, i.e. they are allowed to send public messages.
        /// </summary>
        public string[] Nicks;

        /// <summary>
        /// True if this is the last names reply.
        /// </summary>
        public bool Last;

        public NamesEventArgs(string channel, string[] nicks, bool last)
        {
            Channel = channel;
            Nicks = nicks;
            Last = last;
        }
    }
}