using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnIson"/>.
/// </summary>
public sealed class IsonEventArgs : EventArgs
{
    /// <summary>
    /// The queried nicks that are on the network now. It is empty if none of them are on the
    /// network.
    /// </summary>
    public string Nicks { get; }

    public IsonEventArgs(string nicks) => Nicks = nicks;
}
