namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;

    using FlamingIRC;

    /// <summary>
    /// Purpose: Log text entry by registering to various channel and network/pmsg events
    ///          and writing their output to a text file.       
    /// </summary>
    public static class TextLogger
    {
        // Indicate any IO errors that can't be fixed
        public static event EventHandler<DataEventArgs<string>> WriteFailed;

        // Whether we should add time or not
        public static bool addTimestamp;

        // Timestamp
        public static String timeFormat;

        // Main datastructure
        private static Dictionary<string, Dictionary<string, LoggedItem>> LogFiles =
            new Dictionary<string, Dictionary<String, LoggedItem>>();

        // Logging functions

        public static void TextEntry(Server network, String text)
        {
            WriteText(LogFiles[network.Url]['!' + network.Url], text);
        }

        public static void TextEntry(Server network, User person, String text)
        {
            WriteText(LogFiles[network.Url][person.Nick], text);
        }

        public static void TextEntry(Channel chan, String text)
        {
            WriteText(LogFiles[chan.Server.Url][chan.Name], text);
        }

        private static void WriteText(LoggedItem logger, String text)
        {
            logger.Write(addTimestamp ? DateTime.Now.ToString(timeFormat) + ' ' + text : text);

            // Check for errors
            if (logger.Failed)
            {
                Error(logger.LastError);
            }
        }

        // Error indication
        private static void Error(string err)
        {
            if (WriteFailed != null)
                WriteFailed(null, new DataEventArgs<string>(err));
        }

        // Data structure management
        public static void AddLoggable(Server network)
        {
            // Add value to Network key to hold the different loggables
            LogFiles.Add(network.Url, new Dictionary<string, LoggedItem>());
            // Add the network log
            LogFiles[network.Url].Add('!' + network.Url, new LoggedItem('!' + network.Url, network.Url));
        }

        public static void AddLoggable(Channel chan)
        {
            // Add channel log to the structure
            LogFiles[chan.Server.Url].Add(chan.Name, new LoggedItem(chan.Name, chan.Server.Url));
        }

        public static void AddLoggable(Server network, User person)
        {
            // Add pmsg log to the data struture
            LogFiles[network.Url].Add(person.Nick, new LoggedItem(person.Nick, network.Url)); 
        }

        public static void RemoveLoggable(Server network)
        {
            if (!NetworkExists(network)) return;

            foreach (LoggedItem log in LogFiles[network.Url].Values)
            {
                log.Close();
            }

            LogFiles[network.Url].Clear();
            LogFiles.Remove(network.Url);
        }

        public static void RemoveLoggable(Channel chan)
        {
            if (!ChannelExists(chan)) return;

            LogFiles[chan.Server.Url][chan.Name].Close();
            LogFiles[chan.Server.Url].Remove(chan.Name);
        }

        public static void RemoveLoggable(Server network, User person)
        {
            if (!PersonExists(network, person)) return;

            LogFiles[network.Url][person.Nick].Close();
            LogFiles[network.Url].Remove(person.Nick);
        }

        public static void RemoveAllLoggables()
        {
            foreach(Dictionary<string, LoggedItem> innerDict in LogFiles.Values)
            {
                foreach (LoggedItem li in innerDict.Values)
                {
                    li.Close();
                }

                innerDict.Clear();
            }

            LogFiles.Clear();
        }

        private static bool NetworkExists(Server network)
        {
            return LogFiles.ContainsKey(network.Url);
        }

        private static bool ChannelExists(Channel chan)
        {
            return (NetworkExists(chan.Server) && LogFiles[chan.Server.Url].ContainsKey(chan.Name));
        }

        private static bool PersonExists(Server network, User person)
        {
            return (NetworkExists(network) && LogFiles[network.Url].ContainsKey(person.Nick));
        }
    }
}
