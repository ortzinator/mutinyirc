using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnWhowas"/>.
/// </summary>
public sealed class WhowasEventArgs : EventArgs
{
    /// <summary>
    /// The user that the reply is about.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The real name of the user.
    /// </summary>
    public string RealName { get; }

    /// <summary>
    /// True if this is the last reply. The last reply has no user data.
    /// </summary>
    public bool IsLast { get; }

    public WhowasEventArgs(User user, string realName, bool isLast)
    {
        User = user;
        RealName = realName;
        IsLast = isLast;
    }
}
