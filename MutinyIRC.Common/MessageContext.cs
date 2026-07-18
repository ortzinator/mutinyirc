using System;
using FlamingIRC;

namespace MutinyIRC.Common;

public abstract class MessageContext
{
    /// <summary>
    /// The <see cref="Server"/> this context belongs to. A <see cref="Channel"/> or
    /// <see cref="PrivateMessageSession"/> returns its parent server; a <see cref="Server"/>
    /// returns itself. Lets a command act on the server from any window through a single
    /// <c>Execute(MessageContext, …)</c> overload. (Named <c>OwningServer</c> rather than
    /// <c>Server</c> because a member may not share its enclosing type's name.)
    /// </summary>
    public abstract Server OwningServer { get; }

    /// <summary>
    /// Fired when a NOTICE is sent from this context (the /notice command). The host
    /// echoes it in this context's own window so the outgoing line appears where it was
    /// typed, rather than always in the server window. The event's <c>User</c> carries the
    /// target (a nick or channel name).
    /// </summary>
    public event EventHandler<UserMessageEventArgs> NoticeSent;

    /// <summary>Raises <see cref="NoticeSent"/> for an outgoing notice to <paramref name="target"/>.</summary>
    public void OnNoticeSent(string target, string message)
        => NoticeSent.Fire(this, new UserMessageEventArgs(new User { Nick = target }, message));
}
