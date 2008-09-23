namespace FlamingIRC
{
    using System;

    /// <summary>
    /// Error messages from the IRC server.
    /// </summary>
    public class ErrorMessageEventArgs : EventArgs
    {
        public ReplyCode Code;
        public string Message;

        public ErrorMessageEventArgs(ReplyCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
