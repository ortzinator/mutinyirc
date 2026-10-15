using System;

namespace FlamingIRC;

/// <summary>
/// The data for a message from a user to a channel: <see cref="Listener.OnPublic"/>,
/// <see cref="Listener.OnAction"/>, <see cref="Listener.OnPublicNotice"/> and
/// <see cref="Listener.OnTopicChanged"/>.
/// </summary>
public sealed class UserChannelMessageEventArgs : EventArgs
{
    /// <summary>
    /// The user who sent the message.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The name of the channel.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The text of the message. For <see cref="Listener.OnTopicChanged"/>, the new topic.
    /// </summary>
    public string Message { get; }

    public UserChannelMessageEventArgs(User user, string channel, string message)
    {
        User = user;
        Channel = channel;
        Message = message;
    }
}
