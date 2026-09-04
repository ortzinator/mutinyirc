using System;
using System.Text.Json.Serialization;
using MutinyIRC.Common;

namespace MutinyIRC.UI;

/// <summary>
/// One saved entry point: a host, the port to dial it on, and whether that port speaks TLS. Its
/// position in <see cref="NetworkSettings.Servers"/> is its place in the failover order.
/// </summary>
public class ServerSettings : IEquatable<ServerSettings>
{
    /// <summary>
    /// The serialization default for <see cref="Port"/>, and the fallback for a legacy
    /// <see cref="Ports"/> string. Aliases <see cref="IrcPorts.DefaultPlainPort"/>.
    /// </summary>
    public const int DefaultPort = IrcPorts.DefaultPlainPort;

    public ServerSettings(string url, int port, bool ssl)
    {
        Url = url;
        Port = port;
        Ssl = ssl;
    }

    public ServerSettings() { }

    public string Url { get; set; } = string.Empty;
    public int Port { get; set; } = DefaultPort;
    public bool Ssl { get; set; }

    /// <summary>
    /// Read from disk only, so a file written before the network owned these flags can be folded
    /// up. <see cref="IrcSettingsManager.Load"/> moves the value onto the network, and the
    /// serializer is told to skip the default, so it leaves the file on the next save.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool AutoConnect { get; set; }

    /// <inheritdoc cref="AutoConnect"/>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Nick { get; set; }

    /// <summary>
    /// The port as it was written before one entry point meant one port: a comma-and-range list
    /// of interchangeable candidates. Read from disk only. <see cref="IrcSettingsManager.Load"/>
    /// takes its first value as <see cref="Port"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ports { get; set; }

    [JsonIgnore]
    public NetworkSettings? Network { get; set; }

    /// <summary>
    /// Reads the first port out of a legacy <see cref="Ports"/> string. Only the first ever
    /// mattered: nothing but the random picker read the rest, and nothing read the picker.
    /// Returns <see cref="DefaultPort"/> for anything that does not parse.
    /// </summary>
    public static int FirstPortOf(string? ports)
    {
        if (string.IsNullOrWhiteSpace(ports))
            return DefaultPort;

        string first = ports.Split(',')[0].Split('-')[0].Trim();
        return int.TryParse(first, out int port) && port > 0 && port <= 65535
            ? port
            : DefaultPort;
    }

    public bool Equals(ServerSettings? server) => server != null && server.Url == Url;
}
