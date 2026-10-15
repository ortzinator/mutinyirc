using System;

namespace FlamingIRC;

/// <summary>
/// The data for a user and the text of a message. The Listener uses it for messages to this
/// client: <see cref="Listener.OnPrivate"/>, <see cref="Listener.OnPrivateAction"/> and
/// <see cref="Listener.OnPrivateNotice"/>.
/// </summary>
public sealed class UserMessageEventArgs : EventArgs
{
    /// <summary>
    /// The user who sent the message. For a NOTICE that has no prefix, it is
    /// <see cref="User.Empty"/>.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The text of the message.
    /// </summary>
    public string Message { get; }

    public UserMessageEventArgs(User user, string message)
    {
        User = user;
        Message = message;
    }
}
