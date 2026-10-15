using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnWho"/>. The server sends one reply for each user that
/// matches, then a last reply with no user data.
/// </summary>
public sealed class WhoEventArgs : EventArgs
{
    /// <summary>
    /// The user that the reply is about.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The channel that the user is on.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The name of the IRC server that the user is connected to.
    /// </summary>
    public string IrcServer { get; }

    /// <summary>
    /// The status flags of the user, for example "H" (here) or "G@" (gone, operator).
    /// </summary>
    public string Mask { get; }

    /// <summary>
    /// The number of server hops between you and the user.
    /// </summary>
    public int HopCount { get; }

    /// <summary>
    /// The real name of the user.
    /// </summary>
    public string RealName { get; }

    /// <summary>
    /// True if this is the last reply. The last reply has no user data.
    /// </summary>
    public bool IsLast { get; }

    public WhoEventArgs(User user, string channel, string ircServer, string mask, int hopCount,
                        string realName, bool isLast)
    {
        User = user;
        Channel = channel;
        IrcServer = ircServer;
        Mask = mask;
        HopCount = hopCount;
        RealName = realName;
        IsLast = isLast;
    }
}
