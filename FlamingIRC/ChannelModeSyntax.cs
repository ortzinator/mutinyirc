namespace FlamingIRC;

/// <summary>
/// Tells which channel modes take a parameter on one server. The server announces this in the
/// CHANMODES and PREFIX tokens of its '005' (ISUPPORT) reply.
/// </summary>
/// <remarks>
/// A mode parser that uses the wrong list gives each parameter to the wrong mode. For example,
/// on Libera '+q' is the quiet list and takes a mask. If the parser does not know this, the
/// mask in "+qo *!*@host Nick" goes to 'o', and the nick is lost.
/// </remarks>
internal sealed class ChannelModeSyntax
{
    // The RFC 2811/2812 modes. Use them before the server sends '005', and for each token that
    // the server does not send. The CHANMODES default has the form that a server sends. The
    // PREFIX default holds only the mode letters, because the parser does not read the symbols.
    private const string DefaultPrefixModes = "Oohv";
    private const string DefaultChanModes = "beI,k,l,imnpst";

    // Modes that take a parameter when they are set and when they are unset: the PREFIX modes,
    // and the CHANMODES groups A (lists) and B.
    private readonly string _alwaysModes;

    // Modes that take a parameter only when they are set: the CHANMODES group C.
    private readonly string _setOnlyModes;

    private ChannelModeSyntax(string alwaysModes, string setOnlyModes)
    {
        _alwaysModes = alwaysModes;
        _setOnlyModes = setOnlyModes;
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
        string prefixModes = PrefixModes(properties["PREFIX"]) ?? DefaultPrefixModes;

        string chanModes = properties["CHANMODES"];
        string[] groups = (chanModes.Length > 0 ? chanModes : DefaultChanModes).Split(',');

        return new(prefixModes + Group(groups, 0) + Group(groups, 1), Group(groups, 2));
    }

    internal bool TakesParameter(char mode, ModeAction action) =>
        _alwaysModes.Contains(mode) || (action == ModeAction.Add && _setOnlyModes.Contains(mode));

    // PREFIX has the form "(ohv)@%+". Return the mode letters in the parentheses, or null if
    // the token has a different form.
    private static string PrefixModes(string prefix)
    {
        int close = prefix.IndexOf(')');
        return prefix.StartsWith('(') && close > 0 ? prefix[1..close] : null;
    }

    private static string Group(string[] groups, int index) => index < groups.Length ? groups[index] : "";
}
