namespace FlamingIRC
{
    using System;

    public class DisconnectEventArgs : EventArgs
    {
        public DisconnectReason Reason;
        public int SocketErrorCode;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="socketErrorCode"></param>
        public DisconnectEventArgs(DisconnectReason reason, int socketErrorCode)
        {
            Reason = reason;
            SocketErrorCode = socketErrorCode;
        }

        public DisconnectEventArgs(DisconnectReason reason)
        {
            Reason = reason;
        }
    }
}