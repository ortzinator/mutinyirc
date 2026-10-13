using System.Globalization;
using System.Text.RegularExpressions;

namespace MutinyIRC.Common;

/// <summary>
/// An entry point as the user types it: a host, the port to dial, and whether to use TLS. The
/// Add Server box and <c>/server</c> both read their input through <see cref="TryParse"/>.
/// </summary>
public readonly record struct EntryPoint(string Host, int Port, bool Ssl)
{
    /// <summary>A bare host, or a host and a port. Ported from the mockup's `parseServer()`.</summary>
    private static readonly Regex Pattern =
        new(@"^([A-Za-z0-9.\-_]+)(?::([0-9]{2,5}))?$", RegexOptions.Compiled);

    /// <summary>
    /// Reads "host" or "host:port" out of what the user typed. With no port, the entry point
    /// uses <see cref="IrcPorts.DefaultTlsPort"/>. The port decides TLS through
    /// <see cref="IrcPorts.IsTlsPort"/>. Returns false for text that does not parse, and for a
    /// port outside the valid range.
    /// </summary>
    public static bool TryParse(string text, out EntryPoint entryPoint)
    {
        entryPoint = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        Match match = Pattern.Match(text.Trim());
        if (!match.Success)
            return false;

        // The pattern allows only 2 to 5 ASCII digits, so the parse cannot fail.
        int port = match.Groups[2].Success
            ? int.Parse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture)
            : IrcPorts.DefaultTlsPort;
        if (!IrcPorts.IsValidPort(port))
            return false;

        entryPoint = new EntryPoint(match.Groups[1].Value, port, IrcPorts.IsTlsPort(port));
        return true;
    }
}
