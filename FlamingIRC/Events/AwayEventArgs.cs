using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnAway"/>. This client sent a notice or a private message to
/// a user who is away.
/// </summary>
public sealed class AwayEventArgs : EventArgs
{
    /// <summary>
    /// The nick of the user who is away.
    /// </summary>
    public string Nick { get; }

    /// <summary>
    /// The away message that the user set. It can be empty.
    /// </summary>
    public string AwayMessage { get; }

    public AwayEventArgs(string nick, string awayMessage)
    {
        Nick = nick;
        AwayMessage = awayMessage;
    }
}
