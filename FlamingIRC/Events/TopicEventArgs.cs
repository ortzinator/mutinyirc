using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnReceiveTopic"/>.
/// </summary>
public sealed class TopicEventArgs : EventArgs
{
    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The topic of the channel.
    /// </summary>
    public string Topic { get; }

    public TopicEventArgs(string channel, string topic)
    {
        Channel = channel;
        Topic = topic;
    }
}
