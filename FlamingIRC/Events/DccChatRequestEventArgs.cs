using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccListener.OnDccChatRequest"/>.
/// </summary>
public sealed class DccChatRequestEventArgs : EventArgs
{
    /// <summary>
    /// The user who sent the request, with the address and port to connect to.
    /// </summary>
    public DccUser User { get; }

    public DccChatRequestEventArgs(DccUser user) => User = user;
}
