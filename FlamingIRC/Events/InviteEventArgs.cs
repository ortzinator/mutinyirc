using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnInvite"/> and <see cref="Listener.OnInviteSent"/>.
/// </summary>
public sealed class InviteEventArgs : EventArgs
{
    /// <summary>
    /// For <see cref="Listener.OnInvite"/>, the nick of the user who sent the invite. For
    /// <see cref="Listener.OnInviteSent"/>, the nick of the user who got the invite.
    /// </summary>
    public string Nick { get; }

    /// <summary>
    /// The name of the channel in the invite.
    /// </summary>
    public string Channel { get; }

    public InviteEventArgs(string nick, string channel)
    {
        Nick = nick;
        Channel = channel;
    }
}
