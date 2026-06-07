using System;
using FlamingIRC;

namespace MutinyIRC.Common
{
    public abstract class MessageContext
    {
        /// <summary>
        ///   Fired when a NOTICE is sent from this context (the /notice command). The host
        ///   echoes it in this context's own window so the outgoing line appears where it was
        ///   typed, rather than always in the server window. The event's <c>User</c> carries the
        ///   target (a nick or channel name).
        /// </summary>
        public event EventHandler<UserMessageEventArgs> NoticeSent;

        /// <summary>Raises <see cref="NoticeSent"/> for an outgoing notice to <paramref name="target"/>.</summary>
        public void OnNoticeSent(string target, string message)
            => NoticeSent.Fire(this, new UserMessageEventArgs(new User { Nick = target }, message));
    }
}
