namespace FlamingIRC
{
    using System;

    public class ErrorMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The RFC 2812 or custom numeric code.
        /// </summary>
        public ReplyCode Code;

        /// <summary>
        /// The error message text.
        /// </summary>
        public string Message;

        /// <summary>
        /// Error messages from the IRC server.
        /// </summary>
        /// <param name="code">The RFC 2812 or custom numeric code.</param>
        /// <param name="message">The error message text.</param>
        public ErrorMessageEventArgs(ReplyCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
