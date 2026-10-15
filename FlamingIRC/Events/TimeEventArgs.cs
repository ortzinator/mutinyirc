using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnTime"/>.
/// </summary>
public sealed class TimeEventArgs : EventArgs
{
    /// <summary>
    /// The name of the server and its local time.
    /// </summary>
    public string Time { get; }

    public TimeEventArgs(string time) => Time = time;
}
