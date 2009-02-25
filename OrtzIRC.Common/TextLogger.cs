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

    /// <summary>
    /// Purpose: Log text entry by registering to various channel and network/pmsg events
    ///          and writing their output to a text file.       
    /// </summary>
    public static class TextLogger
    {
        /*    
              Network messages, channel text and private messages can be logged.
              
              By default the output is in ./logs/
              
              /logs/server/$Network.log for network messages;
              /logs/server/#channel.log for channel messages;
              /logs/server/user.log     for user    messages.
         */

        // TEMP: Whether the logging facility is active. NYI.
        public static bool Active { get; private set; }
        // TEMP: Better way to do this? See below
        public static String LastError { get; private set; }

        private static readonly string LogDir = Environment.CurrentDirectory + "\\logs";

        /************************************************************************/
        /* Small explanation for this:
         * 
         * Network --|-- Channel Log
         *           |-- Channel Log
         *           |-- User Log
         *           |-- Network Log
         * 
         * According to RFC, channels may not contain any spaces. Therefore we add
         * one space to the Network Log's index (the first character)
         * 
         * So if we want to access a channel, we know it starts by '#' or '&'
         * (according to RFC). If it's a user, then there will be no '#', no '&'
         * and no spaces as the first character. If those conditions are met,
         * we're touching a user's logs.
         * 
         * If we want the network log, we do ' ' + Server.URI.
        /************************************************************************/
        private static Dictionary<string, Dictionary<string, FileStream>> LogFiles =
            new Dictionary<string, Dictionary<String, FileStream>>();

        // ***** LOGGING HAPPENS HERE *****

        public static void TextEntry(Server Network, String Text)
        {
            CheckWriteServer(Network);

            try
            {
                byte[] WriteMe = GetBytes(Text);
                LogFiles[Network.URI][' ' + Network.URI].Write(WriteMe, 0, WriteMe.Length);
            }
            catch (IOException)
            {
                ResetServer(Network);
                TextEntry(Network, Text);
            }
        }

        public static void TextEntry(Server Network, User Person, String Text)
        {
            CheckWritePerson(Network, Person);

            byte[] WriteMe = GetBytes(Text);

            try
            {
                LogFiles[Network.URI][Person.Nick].Write(WriteMe, 0, WriteMe.Length);
            }
            catch (IOException)
            {
                ResetPerson(Network, Person);
                TextEntry(Network, Person, Text);
            }
    }

        public static void TextEntry(Channel Chan, String Text)
        {
            CheckWriteChannel(Chan);

            try
            {
                byte[] WriteMe = GetBytes(Text);
                LogFiles[Chan.Server.URI][Chan.Name].Write(WriteMe, 0, WriteMe.Length);
            }
            catch (IOException)
            {
                ResetChannel(Chan);
                TextEntry(Chan, Text);
            }
        }


        // ***** MANAGES DATASTRUCT *****

        public static void AddServer(Server Network)
        {
            if (!NetworkExists(Network))
            {
                LogFiles.Add(Network.URI, new Dictionary<string, FileStream>());

                // Create log file
                String prepath = LogDir + "\\" + Network.URI;
                CreateIfNotExists(prepath);

                LogFiles[Network.URI].Add(' ' + Network.URI,
                    new FileStream(prepath + "\\!" + Network.URI + ".log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }
        }

        public static void AddChannel(Channel Chan)
        {
            AddServer(Chan.Server);

            if (!ChannelExists(Chan))
            {
                String prepath = LogDir + "\\" + Chan.Server.URI + "\\";
                CreateIfNotExists(prepath);

                LogFiles[Chan.Server.URI].Add(Chan.Name,
                    new FileStream(prepath + Chan.Name + ".log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }
        }

        public static void AddPerson(Server Network, User Person)
        {
            AddServer(Network);

            if (!PersonExists(Network, Person))
            {
                String prepath = LogDir + "\\" + Network.URI + "\\";
                CreateIfNotExists(prepath);

                LogFiles[Network.URI].Add(Person.Nick,
                    new FileStream(prepath + Person.Nick + ".log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }
        }

        public static void RemoveServer(Server Network)
        {
            if (NetworkExists(Network))
            {
                LogFiles[Network.URI].Clear();
                LogFiles.Remove(Network.URI);
            }
        }

        public static void RemoveChannel(Channel Chan)
        {
            if (NetworkExists(Chan.Server))
            {
                LogFiles[Chan.Server.URI].Remove(Chan.Name);
            }
        }

        public static void RemovePerson(Server Network, User Person)
        {
            if (NetworkExists(Network))
            {
                LogFiles[Network.URI].Remove(Person.Nick);
            }
        }

        // ***** ERROR DETECTION / RECOVERY *****

        private static void ResetServer(Server Network)
        {
            RemoveServer(Network);
            AddServer(Network);
        }

        private static void ResetChannel(Channel Chan)
        {
            RemoveChannel(Chan);
            AddChannel(Chan);
        }

        private static void ResetPerson(Server Network, User Person)
        {
            RemovePerson(Network, Person);
            AddPerson(Network, Person);
        }

        private static void CheckWriteServer(Server Network)
        {
            AddServer(Network);
        }

        private static void CheckWriteChannel(Channel Chan)
        {
            AddChannel(Chan);
        }

        private static void CheckWritePerson(Server Network, User Person)
        {
            AddPerson(Network, Person);
        }

        // ***** UTIL *****

        private static String LogDirPath = Environment.CurrentDirectory + "\\logs" + '\\';
        public static void CreateIfNotExists(String path)
        {
            if (!Directory.Exists(LogDirPath + path))
            {
                Directory.CreateDirectory(LogDirPath + path);
            }
        }

        private static readonly ASCIIEncoding encoding = new ASCIIEncoding();
        private static byte[] GetBytes(String Str)
        {
            return (encoding.GetBytes(Str));
        }

        private static string GetString(byte[] Array)
        {
            return (encoding.GetString(Array));
        }

        // ***** VERIFIES WHETHER THE REQUIRED ELEMENT IS IN THE LOGFILES STRUCTURE ******

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
