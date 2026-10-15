using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnQuit"/>.
/// </summary>
public sealed class QuitEventArgs : EventArgs
{
    /// <summary>
    /// The user who quit.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The quit message. It is empty if the user gave no message.
    /// </summary>
    public string Reason { get; }

    public QuitEventArgs(User user, string reason)
    {
        User = user;
        Reason = reason;
    }
}
