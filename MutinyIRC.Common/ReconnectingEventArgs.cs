using System;

namespace MutinyIRC.Common;

/// <summary>
/// Announces a scheduled automatic reconnect: the attempt runs in <see cref="Delay"/> from now.
/// </summary>
public class ReconnectingEventArgs : EventArgs
{
    public ReconnectingEventArgs(TimeSpan delay, int attempt)
    {
        Delay = delay;
        Attempt = attempt;
    }

    /// <summary>How long the connection waits before it dials again.</summary>
    public TimeSpan Delay { get; private set; }

    /// <summary>
    /// Which try this is, counted from 1, since the connection last registered. It continues to
    /// climb after the backoff delay reaches its cap, so it stays a true count of the tries made
    /// and not a copy of the backoff step.
    /// </summary>
    public int Attempt { get; private set; }
}