namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using OrtzIRC;
    using FlamingIRC;

    public delegate void IOErrorEventHandler(String ex);

    /// <summary>
    /// Purpose: Log text entry by registering to various channel and network/pmsg events
    ///          and writing their output to a text file.       
    /// </summary>
    public static class TextLogger
    {
        // Indicate any IO errors that can't be fixed
        public static event IOErrorEventHandler WriteFailed;

        // Whether we should add time or not
        public static bool AddTimestamp;

        // Timestamp
        public static String TimeFormat;

        // Main datastructure
        private static Dictionary<string, Dictionary<string, LoggedItem>> LogFiles =
            new Dictionary<string, Dictionary<String, LoggedItem>>();

        // Logging functions

        public static void TextEntry(Server network, String text)
        {
            WriteText(LogFiles[network.URL]['!' + network.URL], text);
        }

        public static void TextEntry(Server network, User person, String text)
        {
            WriteText(LogFiles[network.URL][person.Nick], text);
        }

        public static void TextEntry(Channel chan, String text)
        {
            WriteText(LogFiles[chan.Server.URL][chan.Name], text);
        }

        private static void WriteText(LoggedItem logger, String text)
        {
            logger.Write(AddTimestamp ? DateTime.Now.ToString(TimeFormat) + ' ' + text : text);

            // Check for errors
            if (logger.Failed)
            {
                Error(logger.LastError);
            }
        }

        // Error indication
        private static void Error(String err)
        {
            if (WriteFailed != null)
                WriteFailed(err);
        }

        // Data structure management
        public static void AddLoggable(Server network)
        {
            // Add value to Network key to hold the different loggables
            LogFiles.Add(network.URL, new Dictionary<string, LoggedItem>());
            // Add the network log
            LogFiles[network.URL].Add('!' + network.URL, new LoggedItem('!' + network.URL, network.URL));
        }

        public static void AddLoggable(Channel chan)
        {
            // Add channel log to the structure
            LogFiles[chan.Server.URL].Add(chan.Name, new LoggedItem(chan.Name, chan.Server.URL));
        }

        public static void AddLoggable(Server network, User person)
        {
            // Add pmsg log to the data struture
            LogFiles[network.URL].Add(person.Nick, new LoggedItem(person.Nick, network.URL)); 
        }

        public static void RemoveLoggable(Server network)
        {
            if (!NetworkExists(network)) return;

            foreach (LoggedItem Log in LogFiles[network.URL].Values)
            {
                Log.Close();
            }

            LogFiles[network.URL].Clear();
            LogFiles.Remove(network.URL);
        }

        public static void RemoveLoggable(Channel chan)
        {
            if (!ChannelExists(chan)) return;

            LogFiles[chan.Server.URL][chan.Name].Close();
            LogFiles[chan.Server.URL].Remove(chan.Name);
        }

        public static void RemoveLoggable(Server network, User person)
        {
            if (!PersonExists(network, person)) return;

            LogFiles[network.URL][person.Nick].Close();
            LogFiles[network.URL].Remove(person.Nick);
        }

        public static void RemoveAllLoggables()
        {
            foreach(Dictionary<string, LoggedItem> InnerDict in LogFiles.Values)
            {
                foreach (LoggedItem Li in InnerDict.Values)
                {
                    Li.Close();
                }

                InnerDict.Clear();
            }

            LogFiles.Clear();
        }

        private static bool NetworkExists(Server network)
        {
            return LogFiles.ContainsKey(network.URL);
        }

        private static bool ChannelExists(Channel chan)
        {
            return (NetworkExists(chan.Server) && LogFiles[chan.Server.URL].ContainsKey(chan.Name));
        }

        private static bool PersonExists(Server network, User person)
        {
            return (NetworkExists(network) && LogFiles[network.URL].ContainsKey(person.Nick));
        }

        private static bool NtwLogExists(Server network)
        {
            return (NetworkExists(network) && LogFiles[network.URL].ContainsKey('!' + network.URL));
        }
    }
}
