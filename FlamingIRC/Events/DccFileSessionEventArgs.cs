using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccFileSession.OnFileTransferTimeout"/>,
/// <see cref="DccFileSession.OnFileTransferStarted"/>,
/// <see cref="DccFileSession.OnFileTransferInterrupted"/> and
/// <see cref="DccFileSession.OnFileTransferCompleted"/>.
/// </summary>
public sealed class DccFileSessionEventArgs : EventArgs
{
    /// <summary>
    /// The file session that raised the event.
    /// </summary>
    public DccFileSession Session { get; }

    public DccFileSessionEventArgs(DccFileSession session) => Session = session;
}
