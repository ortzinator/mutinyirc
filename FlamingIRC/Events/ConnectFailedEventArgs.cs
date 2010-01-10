namespace FlamingIRC
{
    using System;

    public class ConnectFailedEventArgs : EventArgs
    {
        public ConnectError Reason;
        public int SocketErrorCode;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="socketErrorCode"></param>
        public ConnectFailedEventArgs(ConnectError reason, int socketErrorCode)
        {
            Reason = reason;
            SocketErrorCode = socketErrorCode;
        }

        public ConnectFailedEventArgs(ConnectError reason)
        {
            Reason = reason;
        }
    }
}