using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnJoin"/>.
/// </summary>
public sealed class JoinEventArgs : EventArgs
{
    /// <summary>
    /// The user who joined.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    public JoinEventArgs(User user, string channel)
    {
        User = user;
        Channel = channel;
    }
}
