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
    /// Which try this is in the current run, counted from 1. A registration, a user-initiated
    /// disconnect or a <see cref="Server"/> teardown ends the run and the next one starts at 1
    /// again. It continues to climb after the backoff delay reaches its cap, so it stays a
    /// true count of the tries made.
    /// </summary>
    public int Attempt { get; private set; }
}
