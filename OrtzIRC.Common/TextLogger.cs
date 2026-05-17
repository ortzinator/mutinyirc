using System;
using System.Collections.Generic;
using FlamingIRC;

namespace OrtzIRC.Common
{
    /// <summary>
    ///     Writes IRC text events (server, channel, private message) to log files.
    /// </summary>
    public static class TextLogger
    {
        /// <summary>Whether to prepend a timestamp to each log entry.</summary>
        public static bool AddTimestamp;

        /// <summary>
        ///     Format string passed to <see cref="DateTime.ToString(string)" /> when <see cref="AddTimestamp" /> is
        ///     <c>true</c>.
        /// </summary>
        public static string TimeFormat = "HH:mm:ss";

        private static readonly Dictionary<string, Dictionary<string, LoggedItem>> LogFiles = new();

        /// <summary>Raised when a write operation fails due to an I/O error that cannot be recovered.</summary>
        public static event EventHandler<DataEventArgs<string>> WriteFailed;

        /// <summary>Logs a server-level text entry.</summary>
        public static void TextEntry(Server network, string text)
        {
            WriteText(LogFiles[network.Url]['!' + network.Url], text);
        }

        /// <summary>Logs a private-message text entry.</summary>
        public static void TextEntry(Server network, User person, string text)
        {
            WriteText(LogFiles[network.Url][person.Nick], text);
        }

        /// <summary>Logs a channel text entry.</summary>
        public static void TextEntry(Channel chan, string text)
        {
            WriteText(LogFiles[chan.Server.Url][chan.Name], text);
        }

        private static void WriteText(LoggedItem logger, string text)
        {
            logger.Write(AddTimestamp ? DateTime.Now.ToString(TimeFormat) + ' ' + text : text);

            if (logger.Failed) Error(logger.LastError);
        }

        private static void Error(string err)
        {
            if (WriteFailed != null)
                WriteFailed(null, new DataEventArgs<string>(err));
        }

        /// <summary>Registers a server connection for logging, creating its log file.</summary>
        public static void AddLoggable(Server network)
        {
            LogFiles.Add(network.Url, new Dictionary<string, LoggedItem>());
            LogFiles[network.Url].Add('!' + network.Url, new LoggedItem('!' + network.Url, network.Url));
        }

        /// <summary>Registers a channel for logging, creating its log file.</summary>
        public static void AddLoggable(Channel chan)
        {
            LogFiles[chan.Server.Url].Add(chan.Name, new LoggedItem(chan.Name, chan.Server.Url));
        }

        /// <summary>Registers a private-message session for logging, creating its log file.</summary>
        public static void AddLoggable(Server network, User person)
        {
            LogFiles[network.Url].Add(person.Nick, new LoggedItem(person.Nick, network.Url));
        }

        /// <summary>Closes and removes all log files for a server and its channels.</summary>
        public static void RemoveLoggable(Server network)
        {
            if (!NetworkExists(network)) return;

            foreach (LoggedItem log in LogFiles[network.Url].Values) log.Close();

            LogFiles[network.Url].Clear();
            LogFiles.Remove(network.Url);
        }

        /// <summary>Closes and removes the log file for a channel.</summary>
        public static void RemoveLoggable(Channel chan)
        {
            if (!ChannelExists(chan)) return;

            LogFiles[chan.Server.Url][chan.Name].Close();
            LogFiles[chan.Server.Url].Remove(chan.Name);
        }

        /// <summary>Closes and removes the log file for a private-message session.</summary>
        public static void RemoveLoggable(Server network, User person)
        {
            if (!PersonExists(network, person)) return;

            LogFiles[network.Url][person.Nick].Close();
            LogFiles[network.Url].Remove(person.Nick);
        }

        /// <summary>Closes and removes all log files.</summary>
        public static void RemoveAllLoggables()
        {
            foreach (var innerDict in LogFiles.Values)
            {
                foreach (LoggedItem li in innerDict.Values) li.Close();

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
            return NetworkExists(chan.Server) && LogFiles[chan.Server.Url].ContainsKey(chan.Name);
        }

        private static bool PersonExists(Server network, User person)
        {
            return NetworkExists(network) && LogFiles[network.Url].ContainsKey(person.Nick);
        }
    }
}