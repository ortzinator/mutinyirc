using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnReply"/>. The server sent a numeric reply that is not an
/// error and that no other event is for.
/// </summary>
public sealed class ReplyEventArgs : EventArgs
{
    /// <summary>
    /// The numeric code of the reply.
    /// </summary>
    public ReplyCode ReplyCode { get; }

    /// <summary>
    /// The text of the reply.
    /// </summary>
    public string Message { get; }

    public ReplyEventArgs(ReplyCode code, string message)
    {
        ReplyCode = code;
        Message = message;
    }
}
