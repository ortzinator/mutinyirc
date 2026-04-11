using OrtzIRC.Avalonia.Resources;

namespace OrtzIRC.Avalonia;

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
