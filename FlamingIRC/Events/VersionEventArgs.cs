using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnVersion"/>.
/// </summary>
public sealed class VersionEventArgs : EventArgs
{
    /// <summary>
    /// The version data in the form "[version].[debuglevel] [server] :[comments]".
    /// </summary>
    public string VersionInfo { get; }

    public VersionEventArgs(string versionInfo) => VersionInfo = versionInfo;
}
