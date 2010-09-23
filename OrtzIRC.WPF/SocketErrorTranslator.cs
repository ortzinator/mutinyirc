namespace OrtzIRC.WPF
{
    using System;
    using OrtzIRC.WPF.Resources;

    static class SocketErrorTranslator
    {
        public static string GetMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 10060:
                    return SocketErrorStrings.TimedOut;
                case 10061:
                    return SocketErrorStrings.ConnectionRefused;
                default:
                    return "Socket error code " + errorCode;
            }
        }
    }
}
