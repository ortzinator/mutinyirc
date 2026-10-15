using System;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnNames"/>. The server can send the names of a channel in
/// more than one reply.
/// </summary>
public sealed class NamesEventArgs : EventArgs
{
    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The nicks on the channel in this reply. The last reply has no nicks. A '@' before a nick
    /// shows a channel operator. A '+' before a nick shows a user with voice, who can send
    /// messages to a moderated channel.
    /// </summary>
    public IReadOnlyList<string> Nicks { get; }

    /// <summary>
    /// True if this is the last names reply for the channel.
    /// </summary>
    public bool IsLast { get; }

    public NamesEventArgs(string channel, IEnumerable<string> nicks, bool isLast)
    {
        Channel = channel;
        Nicks = new List<string>(nicks).AsReadOnly();
        IsLast = isLast;
    }
}
