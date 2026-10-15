using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnWhois"/>.
/// </summary>
public sealed class WhoisEventArgs : EventArgs
{
    /// <summary>
    /// All the data that the server sent about the queried nick.
    /// </summary>
    public WhoisInfo WhoisInfo { get; }

    public WhoisEventArgs(WhoisInfo whoisInfo) => WhoisInfo = whoisInfo;
}
