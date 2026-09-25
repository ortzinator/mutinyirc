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
    // Use these RFC 2812 modes before the server sends '005', and for each token the server
    // does not send.
    private const string DefaultPrefixModes = "Oohv";
    private const string DefaultAlwaysModes = "beIk";
    private const string DefaultSetOnlyModes = "l";

    internal static readonly ChannelModeSyntax Default =
        new(DefaultPrefixModes + DefaultAlwaysModes, DefaultSetOnlyModes);

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
    internal static ChannelModeSyntax FromServerProperties(ServerProperties properties)
    {
        string prefixModes = ParsePrefixModes(properties["PREFIX"]) ?? DefaultPrefixModes;

        string chanModes = properties["CHANMODES"];
        if (chanModes.Length == 0)
            return new(prefixModes + DefaultAlwaysModes, DefaultSetOnlyModes);

        string[] groups = chanModes.Split(',');
        return new(prefixModes + Group(groups, 0) + Group(groups, 1), Group(groups, 2));
    }

    internal bool TakesParameter(char mode, ModeAction action) =>
        _alwaysModes.Contains(mode) || (action == ModeAction.Add && _setOnlyModes.Contains(mode));

    // PREFIX has the form "(ohv)@%+". Return the mode letters in the parentheses, or null if
    // the token is missing or has a different form.
    private static string ParsePrefixModes(string prefix)
    {
        int close = prefix.IndexOf(')');
        return prefix.StartsWith('(') && close > 0 ? prefix[1..close] : null;
    }

    private static string Group(string[] groups, int index) => index < groups.Length ? groups[index] : "";
}
