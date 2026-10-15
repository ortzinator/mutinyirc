using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnLinks"/>. The server sends one reply for each linked
/// server, then a last reply with no server data.
/// </summary>
public sealed class LinksEventArgs : EventArgs
{
    /// <summary>
    /// The server name as it appears in IRC queries.
    /// </summary>
    public string Mask { get; }

    /// <summary>
    /// The real hostname of the server.
    /// </summary>
    public string Hostname { get; }

    /// <summary>
    /// The number of hops from this server to the linked server. It is -1 in the last reply.
    /// </summary>
    public int HopCount { get; }

    /// <summary>
    /// Data about the server. This is usually the network name.
    /// </summary>
    public string ServerInfo { get; }

    /// <summary>
    /// True if this is the last reply. The last reply has no server data.
    /// </summary>
    public bool IsLast { get; }

    public LinksEventArgs(string mask, string hostname, int hopCount, string serverInfo,
                          bool isLast)
    {
        Mask = mask;
        Hostname = hostname;
        HopCount = hopCount;
        ServerInfo = serverInfo;
        IsLast = isLast;
    }
}
