using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="DccListener.OnDccGetRequest"/>.
/// </summary>
public sealed class DccGetRequestEventArgs : EventArgs
{
    /// <summary>
    /// The user who asks for the file.
    /// </summary>
    public DccUser User { get; }

    /// <summary>
    /// The name of the file that the user asks for.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// True if the user asks for send-ahead (turbo) mode.
    /// </summary>
    public bool IsTurbo { get; }

    public DccGetRequestEventArgs(DccUser user, string fileName, bool isTurbo)
    {
        User = user;
        FileName = fileName;
        IsTurbo = isTurbo;
    }
}
