using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnPart"/>.
/// </summary>
public sealed class PartEventArgs : EventArgs
{
    /// <summary>
    /// The user who left.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The name of the channel that the user left.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The reason that the user gave. It is empty if the user gave no reason.
    /// </summary>
    public string Reason { get; }

    public PartEventArgs(User user, string channel, string reason)
    {
        User = user;
        Channel = channel;
        Reason = reason;
    }
}
