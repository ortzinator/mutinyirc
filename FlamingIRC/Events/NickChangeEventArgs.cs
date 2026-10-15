using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnNick"/>.
/// </summary>
public sealed class NickChangeEventArgs : EventArgs
{
    /// <summary>
    /// The user who changed nick. <see cref="User.Nick"/> is the old nick.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The new nick.
    /// </summary>
    public string NewNick { get; }

    public NickChangeEventArgs(User user, string newNick)
    {
        User = user;
        NewNick = newNick;
    }
}
