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

        // Main datastructure
        private static Dictionary<string, Dictionary<string, LoggedItem>> LogFiles =
            new Dictionary<string, Dictionary<String, LoggedItem>>();

        // Logging functions

        public static void TextEntry(Server network, String text)
        {
            // Write to file
            LoggedItem Li = LogFiles[network.URI]['!' + network.URI];
            Li.Write(text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
            }
        }

        public static void TextEntry(Server network, User person, String text)
        {
            // Write to file
            LoggedItem Li = LogFiles[network.URI][person.Nick];
            Li.Write(text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
            }
        }

        public static void TextEntry(Channel chan, String text)
        {
            // Write to file
            LoggedItem Li = LogFiles[chan.Server.URI][chan.Name];
            Li.Write(text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
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
            LogFiles.Add(network.URI, new Dictionary<string, LoggedItem>());
            // Add the network log
            LogFiles[network.URI].Add('!' + network.URI, new LoggedItem('!' + network.URI, network.URI));
        }

        public static void AddLoggable(Channel chan)
        {
            // Add channel log to the structure
            LogFiles[chan.Server.URI].Add(chan.Name, new LoggedItem(chan.Name, chan.Server.URI));
        }

        public static void AddLoggable(Server network, User person)
        {
            // Add pmsg log to the data struture
            LogFiles[network.URI].Add(person.Nick, new LoggedItem(person.Nick, network.URI)); 
        }

        public static void RemoveLoggable(Server network)
        {
            if (!NetworkExists(network)) return;

            foreach (LoggedItem Log in LogFiles[network.URI].Values)
            {
                Log.Close();
            }

            LogFiles[network.URI].Clear();
            LogFiles.Remove(network.URI);
        }

        public static void RemoveLoggable(Channel chan)
        {
            if (!ChannelExists(chan)) return;

            LogFiles[chan.Server.URI][chan.Name].Close();
            LogFiles[chan.Server.URI].Remove(chan.Name);
        }

        public static void RemoveLoggable(Server network, User person)
        {
            if (!PersonExists(network, person)) return;

            LogFiles[network.URI][person.Nick].Close();
            LogFiles[network.URI].Remove(person.Nick);
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
            return LogFiles.ContainsKey(network.URI);
        }

        private static bool ChannelExists(Channel chan)
        {
            return (NetworkExists(chan.Server) && LogFiles[chan.Server.URI].ContainsKey(chan.Name));
        }

        private static bool PersonExists(Server network, User person)
        {
            return (NetworkExists(network) && LogFiles[network.URI].ContainsKey(person.Nick));
        }

        private static bool NtwLogExists(Server network)
        {
            return (NetworkExists(network) && LogFiles[network.URI].ContainsKey('!' + network.URI));
        }
    }
}
