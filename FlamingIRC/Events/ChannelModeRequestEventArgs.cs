using System;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnChannelModeRequest"/>.
/// </summary>
public sealed class ChannelModeRequestEventArgs : EventArgs
{
    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// All the modes that are set on the channel, with their parameters.
    /// </summary>
    public IReadOnlyList<ChannelModeInfo> Modes { get; }

    public ChannelModeRequestEventArgs(string channel, IEnumerable<ChannelModeInfo> modes)
    {
        Channel = channel;
        Modes = new List<ChannelModeInfo>(modes).AsReadOnly();
    }
}
