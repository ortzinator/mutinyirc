namespace OrtzIRC
{
    using System;
    using OrtzIRC.Common;

    public class PrivateMessageSessionEventArgs : EventArgs
    {
        public PrivateMessageSessionEventArgs(PrivateMessageSession pmsession)
        {
            PrivateMessageSession = pmsession;
        }

        public PrivateMessageSession PrivateMessageSession { get; private set; }
    }
}
