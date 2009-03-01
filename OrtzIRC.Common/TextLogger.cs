namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using FlamingIRC;
    using OrtzIRC;

    /// <summary>
    /// Purpose: Handle the environnement around the manager such as subscribing
    ///          to program events and managing whether the logger is on or off.
    ///          it also listens to the various events in the program in order
    ///          to detect any text input and tell the logger what to do.
    /// </summary>
    public static class TextLoggerManager
    {
        // This should be changed to forms but I'm trying to grasp the codebase atm
        // Doesn't look like there's a clean way to do it, and I don't want to refactor
        // code that isn't mine too much.
        public static void ServerWindowOpened(Server Network)
        {
            // Register to AddLine event
        }

        public static void ServerWindowClosed(Server Network)
        {
            // Unregister AddLine event (Might be unnecessary)
        }

        public static void ChannelWindowOpened(Channel Chan)
        {
            // Register to AddLine event            
        }

        public static void ChannelWindowClosed(Channel Chan)
        {
            // Unregister AddLine event (Might be unnecessary)
        }    
    }

    public delegate void IOErrorEventHandler(String ex);

    /// <summary>
    /// Purpose: Log text entry by registering to various channel and network/pmsg events
    ///          and writing their output to a text file.       
    /// </summary>
    public static class TextLogger
    {
        // TEMP: Whether the logging facility is active. NYI.
        public static bool Active { get; private set; }

        // Indicate any IO errors that can't be fixed
        public static event IOErrorEventHandler WriteFailed;

        // Main datastructure
        private static Dictionary<string, Dictionary<string, LoggedItem>> LogFiles =
            new Dictionary<string, Dictionary<String, LoggedItem>>();

        // Logging functions

        public static void TextEntry(Server Network, String Text)
        {
            // Verify the network has been added
            if (!NetworkExists(Network))
            {
                AddLoggable(Network);
            }

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
            // Verify the network has been added
            if (!NetworkExists(Network))
            {
                AddLoggable(Network);
            }

            // Verify the person has been added
            if (!PersonExists(Network, Person))
            {
                AddLoggable(Network, Person);
            }

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
            // Verify the network has been added
            if (!NetworkExists(Chan.Server))
            {
                AddLoggable(Chan.Server);
            }

            // Verify the channel has been added
            if (!ChannelExists(Chan))
            {
                AddLoggable(Chan);
            }

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
        private static void AddLoggable(Server Network)
        {
            // Add value to Network key to hold the different loggables
            LogFiles.Add(Network.URI, new Dictionary<string, LoggedItem>());
            // Add the network log
            LogFiles[Network.URI].Add('!' + Network.URI, new LoggedItem('!' + Network.URI, Network.URI));
        }

        private static void AddLoggable(Channel Chan)
        {
            // Add channel log to the structure
            LogFiles[Chan.Server.URI].Add(Chan.Name, new LoggedItem('#' + Chan.Name, Chan.Server.URI));
        }

        private static void AddLoggable(Server Network, User Person)
        {
            // Add pmsg log to the data struture
            LogFiles[Network.URI].Add(Person.Nick, new LoggedItem(Person.Nick, Network.URI)); 
        }

        private static void RemoveLoggable(Server Network)
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

        private static void RemoveLoggable(Channel Chan)
        {
            LogFiles[Chan.Server.URI].Remove(Chan.Name);
        }

        private static void RemoveLoggable(Server Network, User Person)
        {
            LogFiles[Network.URI].Remove(Person.Nick);
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
            return (NetworkExists(Network) && LogFiles[Network.URI].ContainsKey(' ' + Network.URI));
        }
    }
}
