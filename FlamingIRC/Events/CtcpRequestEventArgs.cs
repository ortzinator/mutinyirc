using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="CtcpListener.OnCtcpRequest"/>.
/// </summary>
public sealed class CtcpRequestEventArgs : EventArgs
{
    /// <summary>
    /// The CTCP command in upper case, for example "VERSION". See <see cref="CtcpUtil"/>.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// The user who sent the request.
    /// </summary>
    public User User { get; }

    public CtcpRequestEventArgs(string command, User user)
    {
        Command = command;
        User = user;
    }
}
