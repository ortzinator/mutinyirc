using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnStats"/>.
/// </summary>
public sealed class StatsEventArgs : EventArgs
{
    /// <summary>
    /// The type of query that this reply answers.
    /// </summary>
    public StatsQuery QueryType { get; }

    /// <summary>
    /// The text of the reply.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// True if this is the last reply.
    /// </summary>
    public bool IsLast { get; }

    public StatsEventArgs(StatsQuery queryType, string message, bool isLast)
    {
        QueryType = queryType;
        Message = message;
        IsLast = isLast;
    }
}
