using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccFileSession.OnFileTransferProgress"/>.
/// </summary>
public sealed class DccFileProgressEventArgs : EventArgs
{
    /// <summary>
    /// The file session that sent or received the data.
    /// </summary>
    public DccFileSession Session { get; }

    /// <summary>
    /// The number of bytes in this block only. For the total number of bytes sent or received,
    /// and the size of the file, see <see cref="DccFileSession.File"/>.
    /// </summary>
    public int BytesTransferred { get; }

    public DccFileProgressEventArgs(DccFileSession session, int bytesTransferred)
    {
        Session = session;
        BytesTransferred = bytesTransferred;
    }
}
