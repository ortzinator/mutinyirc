using System;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnChannelModeChange"/>.
/// </summary>
public sealed class ChannelModeChangeEventArgs : EventArgs
{
    /// <summary>
    /// The user who changed the modes.
    /// </summary>
    public User Changer { get; }

    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The mode changes, one item for each mode, with their parameters.
    /// </summary>
    public IReadOnlyList<ChannelModeInfo> Modes { get; }

    /// <summary>
    /// The mode string as the server sent it, for example "+ov Alice Bob".
    /// </summary>
    public string Raw { get; }

    public ChannelModeChangeEventArgs(User changer, string channel,
                                      IEnumerable<ChannelModeInfo> modes, string raw)
    {
        Changer = changer;
        Channel = channel;
        Modes = new List<ChannelModeInfo>(modes).AsReadOnly();
        Raw = raw;
    }
}
