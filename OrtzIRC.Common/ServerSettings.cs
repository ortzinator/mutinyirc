using System;
using System.Collections.Generic;

namespace OrtzIRC.Common
{
    public class ServerSettings
    {
        public ServerSettings(string uri, string description, string ports, bool ssl)
        {
            Uri = uri;
            Description = description;
            Ports = ports;
            Ssl = ssl;
        }

        public ServerSettings() { }

        public string Uri { get; set; }
        public string Description { get; set; }
        public string Ports { get; set; }
        public bool Ssl { get; set; }
        public int Id { get; set; }

        private int[] PortList
        {
            get
            {
                return PortsStringToArray(Ports);
            }
        }

        private static int[] PortsStringToArray(string ports)
        {
            string[] portListChunk = ports.Split(',');

            List<int> portList = new List<int>(portListChunk.Length); // Maximum performance if there are no ranges

            foreach (string chunk in portListChunk)
            {
                if (chunk.Contains("-"))
                {
                    // Chunk is a range
                    string[] rangeParts = chunk.Split('-');

                    // User entered something stupid like "30-;"
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

                    // User entered something dumb like "30-20" or "20-20"
                    if (endRange <= beginRange)
                        throw new FormatException();

                    // Add entire range to the list
                    while (beginRange <= endRange && !portList.Contains(beginRange))
                        portList.Add(beginRange++);
                }
                else // Chunk is a single port number
                {
                    int tempNum;

                    try
                    {
                        tempNum = Int32.Parse( chunk );
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
    }
}
