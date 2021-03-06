﻿namespace FlamingIRC
{
    using System;

    public class AwayEventArgs : EventArgs
    {
        /// <summary>
        /// The nick of the user who is away.
        /// </summary>
        public string Nick;

        /// <summary>
        /// An away message, if any, set by the user. 
        /// </summary>
        public string AwayMessage;

        /// <summary>
        /// A Notice or Private message was sent to someone whose status is away.
        /// </summary>
        /// <param name="nick">The nick of the user who is away.</param>
        /// <param name="awayMessage">An away message, if any, set by the user.</param>
        public AwayEventArgs(string nick, string awayMessage)
        {
            Nick = nick;
            AwayMessage = awayMessage;
        }
    }
}