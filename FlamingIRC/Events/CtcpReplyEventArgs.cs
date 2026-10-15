using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="CtcpListener.OnCtcpReply"/>.
/// </summary>
public sealed class CtcpReplyEventArgs : EventArgs
{
    /// <summary>
    /// The CTCP command that the reply answers, in upper case, for example "VERSION".
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// The user who sent the reply.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// The text of the reply.
    /// </summary>
    public string Reply { get; }

    public CtcpReplyEventArgs(string command, User user, string reply)
    {
        Command = command;
        User = user;
        Reply = reply;
    }
}
