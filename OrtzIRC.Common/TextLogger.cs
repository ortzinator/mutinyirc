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
        // TEMP: Whether the logging facility is active. NYI.
        public static bool Active = true;

        // Indicate any IO errors that can't be fixed
        public static event IOErrorEventHandler WriteFailed;

        // Main datastructure
        private static Dictionary<string, Dictionary<string, LoggedItem>> LogFiles =
            new Dictionary<string, Dictionary<String, LoggedItem>>();

        // Logging functions

        public static void TextEntry(Server Network, String Text)
        {
            // Write to file
            LoggedItem Li = LogFiles[Network.URI]['!' + Network.URI];
            Li.Write(Text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
            }
        }

        public static void TextEntry(Server Network, User Person, String Text)
        {
            // Write to file
            LoggedItem Li = LogFiles[Network.URI][Person.Nick];
            Li.Write(Text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
            }
        }

        public static void TextEntry(Channel Chan, String Text)
        {
            // Write to file
            LoggedItem Li = LogFiles[Chan.Server.URI][Chan.Name];
            Li.Write(Text);

            // Check for errors
            if (Li.Failed)
            {
                Error(Li.LastError);
            }
        }

        // Error indication

        private static void Error(String Err)
        {
            if (WriteFailed != null)
                WriteFailed(Err);
        }

        // Data structure management
        public static void AddLoggable(Server Network)
        {
            // Add value to Network key to hold the different loggables
            LogFiles.Add(Network.URI, new Dictionary<string, LoggedItem>());
            // Add the network log
            LogFiles[Network.URI].Add('!' + Network.URI, new LoggedItem('!' + Network.URI, Network.URI));
        }

        public static void AddLoggable(Channel Chan)
        {
            // Add channel log to the structure
            LogFiles[Chan.Server.URI].Add(Chan.Name, new LoggedItem(Chan.Name, Chan.Server.URI));
        }

        public static void AddLoggable(Server Network, User Person)
        {
            // Add pmsg log to the data struture
            LogFiles[Network.URI].Add(Person.Nick, new LoggedItem(Person.Nick, Network.URI)); 
        }

        public static void RemoveLoggable(Server Network)
        {
            if (NetworkExists(Network))
            {
                foreach (LoggedItem Log in LogFiles[Network.URI].Values)
                {
                    Log.Close();
                }

                LogFiles[Network.URI].Clear();
                LogFiles.Remove(Network.URI);
            }
        }

        public static void RemoveLoggable(Channel Chan)
        {
            if (ChannelExists(Chan))
            {
                LogFiles[Chan.Server.URI][Chan.Name].Close();
                LogFiles[Chan.Server.URI].Remove(Chan.Name);
            }
        }

        public static void RemoveLoggable(Server Network, User Person)
        {
            if (PersonExists(Network, Person))
            {
                LogFiles[Network.URI][Person.Nick].Close();
                LogFiles[Network.URI].Remove(Person.Nick);
            }
        }

        private static bool NetworkExists(Server Network)
        {
            return LogFiles.ContainsKey(Network.URI);
        }

        private static bool ChannelExists(Channel Chan)
        {
            return (NetworkExists(Chan.Server) && LogFiles[Chan.Server.URI].ContainsKey(Chan.Name));
        }

        private static bool PersonExists(Server Network, User Person)
        {
            return (NetworkExists(Network) && LogFiles[Network.URI].ContainsKey(Person.Nick));
        }

        private static bool NtwLogExists(Server Network)
        {
            return (NetworkExists(Network) && LogFiles[Network.URI].ContainsKey('!' + Network.URI));
        }
    }
}
