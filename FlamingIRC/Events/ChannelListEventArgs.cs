using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnChannelList"/>. The server sends one reply for each
/// item in the list, then a last reply.
/// </summary>
public sealed class ChannelListEventArgs : EventArgs
{
    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The type of list, for example bans or invitation masks.
    /// </summary>
    public ChannelMode Mode { get; }

    /// <summary>
    /// A mask, or a nick when <see cref="Mode"/> is <see cref="ChannelMode.ChannelCreator"/>.
    /// It is empty in the last reply, except for <see cref="ChannelMode.ChannelCreator"/>.
    /// </summary>
    public string Item { get; }

    /// <summary>
    /// The user who set the mask. It is <see cref="User.Empty"/> for
    /// <see cref="ChannelMode.ChannelCreator"/> and in the last reply.
    /// </summary>
    public User Setter { get; }

    /// <summary>
    /// The time when the mask was set, in seconds since the Unix epoch. It is 0 for
    /// <see cref="ChannelMode.ChannelCreator"/> and in the last reply.
    /// </summary>
    public long WhenSet { get; }

    /// <summary>
    /// True if this is the last reply.
    /// </summary>
    public bool IsLast { get; }

    public ChannelListEventArgs(string channel, ChannelMode mode, string item, User setter,
                                long whenSet, bool isLast)
    {
        Channel = channel;
        Mode = mode;
        Item = item;
        Setter = setter;
        WhenSet = whenSet;
        IsLast = isLast;
    }
}
