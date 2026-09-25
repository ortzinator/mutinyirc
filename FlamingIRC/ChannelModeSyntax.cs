namespace FlamingIRC;

/// <summary>
/// Tells which channel modes take a parameter on one server, and which status symbol each
/// prefix mode gives. The server announces this in the CHANMODES and PREFIX tokens of its '005'
/// (ISUPPORT) reply.
/// </summary>
/// <remarks>
/// A mode parser that uses the wrong list gives each parameter to the wrong mode. For example,
/// on Libera '+q' is the quiet list and takes a mask. If the parser does not know this, the
/// mask in "+qo *!*@host Nick" goes to 'o', and the nick is lost.
/// </remarks>
internal sealed class ChannelModeSyntax
{
    // The RFC 2811/2812 modes, in the form that a server sends them. Use them before the server
    // sends '005', and for each token that the server does not send. 'O' (channel creator)
    // takes a nick but gives no status symbol, so it is in group B and not in PREFIX.
    private const string DefaultPrefix = "(ohv)@%+";
    private const string DefaultChanModes = "beI,kO,l,imnpst";

    // The PREFIX modes, and at the same index, the status symbol that each one gives.
    private readonly string _prefixModes;
    private readonly string _prefixSymbols;

    // Modes other than PREFIX modes that take a parameter when they are set and when they are
    // unset: the CHANMODES groups A (lists) and B.
    private readonly string _alwaysModes;

    // Modes that take a parameter only when they are set: the CHANMODES group C.
    private readonly string _setOnlyModes;

    private ChannelModeSyntax(string prefix, string[] chanModeGroups)
    {
        int close = prefix.IndexOf(')');
        _prefixModes = prefix[1..close];
        _prefixSymbols = prefix[(close + 1)..];
        _alwaysModes = Group(chanModeGroups, 0) + Group(chanModeGroups, 1);
        _setOnlyModes = Group(chanModeGroups, 2);
    }

    /// <summary>
    /// Reads CHANMODES and PREFIX from the server's '005' properties. The CHANMODES group D and
    /// modes that no token names take no parameter.
    /// </summary>
    /// <remarks>
    /// <see cref="ServerProperties"/> gives an empty string both for a missing token and for an
    /// empty one, such as "PREFIX=". Both use the default here.
    /// </remarks>
    internal static ChannelModeSyntax FromServerProperties(ServerProperties properties)
    {
        string prefix = properties["PREFIX"];
        string chanModes = properties["CHANMODES"];

        return new(IsWellFormedPrefix(prefix) ? prefix : DefaultPrefix,
                   (chanModes.Length > 0 ? chanModes : DefaultChanModes).Split(','));
    }

    internal bool TakesParameter(char mode, ModeAction action) =>
        _prefixModes.Contains(mode) || _alwaysModes.Contains(mode) ||
        (action == ModeAction.Add && _setOnlyModes.Contains(mode));

    /// <summary>
    /// The status symbol that a PREFIX mode gives, for example '@' for 'o'. '\0' for a mode
    /// that is not in PREFIX.
    /// </summary>
    internal char PrefixSymbol(char mode)
    {
        int index = _prefixModes.IndexOf(mode);
        return index < 0 ? '\0' : _prefixSymbols[index];
    }

    // PREFIX has the form "(ohv)@%+", with one symbol for each mode letter.
    private static bool IsWellFormedPrefix(string prefix)
    {
        int close = prefix.IndexOf(')');
        return prefix.StartsWith('(') && close > 0 && prefix.Length - close - 1 == close - 1;
    }

    private static string Group(string[] groups, int index) => index < groups.Length ? groups[index] : "";
}
