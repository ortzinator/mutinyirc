using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccChatSession.OnChatSessionOpened"/>,
/// <see cref="DccChatSession.OnChatSessionClosed"/> and
/// <see cref="DccChatSession.OnChatRequestTimeout"/>.
/// </summary>
public sealed class DccChatSessionEventArgs : EventArgs
{
    /// <summary>
    /// The chat session that raised the event.
    /// </summary>
    public DccChatSession Session { get; }

    public DccChatSessionEventArgs(DccChatSession session) => Session = session;
}
