using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="CtcpListener.OnCtcpPingRequest"/> and
/// <see cref="CtcpListener.OnCtcpPingReply"/>.
/// </summary>
public sealed class CtcpPingEventArgs : EventArgs
{
    /// <summary>
    /// The user who sent the ping request or the ping reply.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The timestamp from the ping request. A reply must send it back without change.
    /// </summary>
    public string Timestamp { get; }

    public CtcpPingEventArgs(User user, string timestamp)
    {
        User = user;
        Timestamp = timestamp;
    }
}
