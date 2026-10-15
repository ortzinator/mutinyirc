using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnAdmin"/>.
/// </summary>
public sealed class AdminEventArgs : EventArgs
{
    /// <summary>
    /// One line of data about the server administrator, for example a location or an email
    /// address.
    /// </summary>
    public string Message { get; }

    public AdminEventArgs(string message) => Message = message;
}
