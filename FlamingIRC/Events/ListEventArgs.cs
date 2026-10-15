using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnList"/>.
/// </summary>
public sealed class ListEventArgs : EventArgs
{
    /// <summary>
    /// The name of the channel. It is empty when <see cref="IsLast"/> is true.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The number of users on the channel that you can see.
    /// </summary>
    public int VisibleNickCount { get; }

    /// <summary>
    /// The topic of the channel.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// True if this is the end of the list. The last reply has no channel data.
    /// </summary>
    public bool IsLast { get; }

    public ListEventArgs(string channel, int visibleNickCount, string topic, bool isLast)
    {
        Channel = channel;
        VisibleNickCount = visibleNickCount;
        Topic = topic;
        IsLast = isLast;
    }
}
