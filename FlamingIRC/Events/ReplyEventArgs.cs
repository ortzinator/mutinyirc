namespace FlamingIRC
{
    using System;

    /// <summary>
    /// Messages that are not handled by other events and are not errors.
    /// </summary>
    public class ReplyEventArgs : EventArgs
    {
        public ReplyCode ReplyCode;
        public string Message;

        public ReplyEventArgs(ReplyCode code, string message)
        {
            ReplyCode = code;
            Message = message;
        }
    }
}
