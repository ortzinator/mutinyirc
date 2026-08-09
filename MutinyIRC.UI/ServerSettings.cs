using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MutinyIRC.UI;

public class ServerSettings : IEquatable<ServerSettings>
{
    public ServerSettings(string url, string description, string ports, bool ssl)
    {
        Url = url;
        Description = description;
        Ports = ports;
        Ssl = ssl;
        AutoConnect = true;
    }

    public ServerSettings() { }

    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ports { get; set; } = string.Empty;
    public bool Ssl { get; set; }
    public bool AutoConnect { get; set; }
    public string? Nick { get; set; }
    [JsonIgnore]
    public NetworkSettings? Network { get; set; }

    public int RandomPort
    {
        get
        {
            var list = PortList;
            if (list == null || list.Length == 0)
                throw new InvalidOperationException("No ports configured.");
            // The upper bound is exclusive, so it is the length, not the last index.
            return list[new Random().Next(0, list.Length)];
        }
    }

    private int[]? PortList => PortsStringToArray(Ports);

    private static int[]? PortsStringToArray(string ports)
    {
        if (ports == string.Empty)
            return null;

        string[] portListChunk = ports.Split(',');
        var portList = new List<int>(portListChunk.Length);

        foreach (string chunk in portListChunk)
        {
            if (chunk.Contains("-"))
            {
                string[] rangeParts = chunk.Split('-');
                if (rangeParts.Length != 2)
                    throw new FormatException();

                int beginRange;
                int endRange;

                try
                {
                    beginRange = Int32.Parse(rangeParts[0]);
                    endRange = Int32.Parse(rangeParts[1]);
                }
                catch (Exception)
                {
                    throw new FormatException();
                }

                if (endRange <= beginRange)
                    throw new FormatException();

                while (beginRange <= endRange && !portList.Contains(beginRange))
                    portList.Add(beginRange++);
            }
            else
            {
                int tempNum;
                try
                {
                    tempNum = Int32.Parse(chunk);
                }
                catch (Exception)
                {
                    throw new FormatException();
                }

                if (!portList.Contains(tempNum))
                    portList.Add(tempNum);
            }
        }

        return portList.ToArray();
    }

    public bool Equals(ServerSettings? server) => server != null && server.Url == Url;
}
