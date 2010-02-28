namespace OrtzIRC
{
    using System;
    using OrtzIRC.Resources;

    static class SocketErrorTranslator
    {
        public static string GetMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 10060:
                    return SocketErrorStrings.TimedOut;
                default:
                    return "Socket error code " + errorCode;
            }
        }
    }
}
