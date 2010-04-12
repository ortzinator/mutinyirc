namespace OrtzIRC.Common
{
    using System;

    public class PrivateMessageSessionEventArgs : EventArgs
    {
        public PrivateMessageSessionEventArgs(PrivateMessageSession pmsession)
        {
            PrivateMessageSession = pmsession;
        }

        public PrivateMessageSession PrivateMessageSession { get; private set; }
    }
}
