using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccListener.OnDccSendRequest"/>.
/// </summary>
public sealed class DccSendRequestEventArgs : EventArgs
{
    /// <summary>
    /// The user who offers the file, with the address and port to connect to.
    /// </summary>
    public DccUser User { get; }

    /// <summary>
    /// The name of the file that the user offers.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// True if the sender uses send-ahead (turbo) mode.
    /// </summary>
    public bool IsTurbo { get; }

    public DccSendRequestEventArgs(DccUser user, string fileName, int size, bool isTurbo)
    {
        User = user;
        FileName = fileName;
        Size = size;
        IsTurbo = isTurbo;
    }
}
