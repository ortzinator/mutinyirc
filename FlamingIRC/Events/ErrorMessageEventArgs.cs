using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnError"/>. The IRC server sent an error, or FlamingIRC
/// found an error.
/// </summary>
public sealed class ErrorMessageEventArgs : EventArgs
{
    /// <summary>
    /// The RFC 2812 numeric code, or a FlamingIRC code.
    /// </summary>
    public ReplyCode Code { get; }

    /// <summary>
    /// The text of the error.
    /// </summary>
    public string Message { get; }

    public ErrorMessageEventArgs(ReplyCode code, string message)
    {
        Code = code;
        Message = message;
    }
}
