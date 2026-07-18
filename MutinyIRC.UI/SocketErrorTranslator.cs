using MutinyIRC.UI.Resources;

namespace MutinyIRC.UI;

internal static class SocketErrorTranslator
{
    public static string GetMessage(int errorCode)
    {
        return errorCode switch
        {
            10060 => SocketErrorStrings.TimedOut,
            10061 => SocketErrorStrings.ConnectionRefused,
            _ => "Socket error code " + errorCode,
        };
    }
}
