using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnKill"/>.
/// </summary>
public sealed class KillEventArgs : EventArgs
{
    /// <summary>
    /// The operator or server that sent the KILL.
    /// </summary>
    public User Killer { get; }

    /// <summary>
    /// The nick of the user who was disconnected.
    /// </summary>
    public string Nick { get; }

    /// <summary>
    /// The reason for the KILL. It is empty if the KILL gave no reason.
    /// </summary>
    public string Reason { get; }

    public KillEventArgs(User killer, string nick, string reason)
    {
        Killer = killer;
        Nick = nick;
        Reason = reason;
    }
}
