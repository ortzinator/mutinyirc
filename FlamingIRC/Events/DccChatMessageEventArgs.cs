using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccChatSession.OnChatMessageReceived"/>.
/// </summary>
public sealed class DccChatMessageEventArgs : EventArgs
{
    /// <summary>
    /// The chat session that received the message.
    /// </summary>
    public DccChatSession Session { get; }

    /// <summary>
    /// The text that the remote user sent.
    /// </summary>
    public string Message { get; }

    public DccChatMessageEventArgs(DccChatSession session, string message)
    {
        Session = session;
        Message = message;
    }
}
