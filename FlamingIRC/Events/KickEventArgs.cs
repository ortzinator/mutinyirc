using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnKick"/>.
/// </summary>
public sealed class KickEventArgs : EventArgs
{
    /// <summary>
    /// The user who did the kick.
    /// </summary>
    public User Kicker { get; }

    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The nick of the user whom the kicker removed from the channel.
    /// </summary>
    public string Kickee { get; }

    /// <summary>
    /// The reason for the kick.
    /// </summary>
    public string Reason { get; }

    public KickEventArgs(User kicker, string channel, string kickee, string reason)
    {
        Kicker = kicker;
        Channel = channel;
        Kickee = kickee;
        Reason = reason;
    }
}
