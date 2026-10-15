using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnNickError"/>. The server did not accept a nick.
/// </summary>
/// <remarks>
/// <para>This occurs in two conditions. The user is registered with the server and tries to
/// change nick. Or the user tries to register with the server and the nick is not accepted.</para>
/// <para>In the second condition, you must complete the registration yourself.</para>
/// </remarks>
public sealed class NickErrorEventArgs : EventArgs
{
    /// <summary>
    /// The nick that the server did not accept.
    /// </summary>
    public string BadNick { get; }

    /// <summary>
    /// The text from the server that tells why it did not accept the nick.
    /// </summary>
    public string Reason { get; }

    public NickErrorEventArgs(string badNick, string reason)
    {
        BadNick = badNick;
        Reason = reason;
    }
}
