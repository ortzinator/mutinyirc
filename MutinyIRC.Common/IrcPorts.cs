namespace MutinyIRC.Common;

/// <summary>
/// One source of truth for the port conventions the client assumes: which port to dial when the
/// user names none, and which ports mean plaintext rather than TLS. Callers route through here so
/// the rule lives in one place and cannot drift.
/// </summary>
public static class IrcPorts
{
    /// <summary>The historical plaintext port, and the fallback when nothing else is known.</summary>
    public const int DefaultPlainPort = 6667;

    /// <summary>The conventional TLS port, and the sensible default for a network added today.</summary>
    public const int DefaultTlsPort = 6697;

    /// <summary>
    /// The ports that historically mean plaintext. Every other port is assumed to be TLS. Both
    /// values come from the mockup's parseServer().
    /// </summary>
    private static readonly int[] PlainPorts = { 6665, DefaultPlainPort };

    /// <summary>
    /// Whether a port should be assumed to speak TLS. The port is the only thing known about a
    /// host that has never been dialled, so the two plaintext ports decide: everything else is TLS.
    /// </summary>
    public static bool IsTlsPort(int port) => !IsPlainPort(port);

    /// <summary>Whether a port is one of the two that historically mean plaintext.</summary>
    public static bool IsPlainPort(int port)
    {
        foreach (int plain in PlainPorts)
        {
            if (plain == port)
                return true;
        }

        return false;
    }
}
