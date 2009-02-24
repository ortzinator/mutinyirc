namespace OrtzIRC.Common
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using FlamingIRC;

    /// <summary>
    /// Purpose: Log text entry by registering to various channel and network/pmsg events
    ///          and writing their output to a text file.       
    /// </summary>
    class TextLogger
    {
        /*       
              Network messages, channel text and private messages can be logged.
              
              By default the output is in ./logs/
              
              /logs/server/$Network.log for network messages;
              /logs/server/#channel.log for channel messages;
              /logs/server/user.log     for user    messages.
         */

        // TEMP: Whether the logging facility is active. NYI.
        public bool Active { get; private set; }
        // TEMP: Better way to do this? See below
        public String LastError { get; private set; }

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
        Dictionary<string, Dictionary<string, TextWriter>> LogFiles = 
            new Dictionary<string, Dictionary<String, TextWriter>>();

        #region Logging

        // ***** LOGGING HAPPENS HERE *****

        void TextEntry(Server Network, String Text)
        {
            if (NtwLogExists(Network))
            {
                try
                {
                    LogFiles[Network.URI][' ' + Network.URI].WriteLine(Text);
                }
                catch (IOException ex)
                {
                    // TODO: Should we throw? I might see while implementing logging registration later.
                    Active = false;
                    LastError = ex.ToString();
                }
            }
        }

        void TextEntry(Server Network, User Person, String Text)
        {
            if (PersonExists(Network, Person))
            {
                try
                {
                    LogFiles[Network.URI][Person.Nick].WriteLine(Text);
                }
                catch (IOException ex)
                {
                    // TODO: Should we throw? I might see while implementing logging registration later.
                    Active = false;
                    LastError = ex.ToString();
                }
            }
        }

        void TextEntry(Channel Chan, String Text)
        {
            if (ChannelExists(Chan))
            {
                try
                {
                    LogFiles[Chan.Server.URI][Chan.Name].WriteLine(Text);
                }
                catch (IOException ex)
                {
                    // TODO: Should we throw? I might see while implementing logging registration later.
                    Active = false;
                    LastError = ex.ToString();
                }
            }
            else
            {
                // Enforce API
                throw new InvalidOperationException("Use log registration to write to log!");
            }
        }

        #endregion

        #region Exists

        // ***** VERIFIES WHETHER THE REQUIRED ELEMENT IS IN THE LOGFILES STRUCTURE ******

        bool NetworkExists(Server Network)
        {
            return LogFiles.ContainsKey(Network.URI);
        }

        bool ChannelExists(Channel Chan)
        {
            return (NetworkExists(Chan.Server) && LogFiles[Chan.Server.URI].ContainsKey(Chan.Name));
        }

        bool PersonExists(Server Network, User Person)
        {
            return (NetworkExists(Network) && LogFiles[Network.URI].ContainsKey(Person.Nick));
        }

        bool NtwLogExists(Server Network)
        {
            return (NetworkExists(Network) && LogFiles[Network.URI].ContainsKey(' ' + Network.URI));
        }

        #endregion

        #region Add/Remove

        // ***** MANAGES DATASTRUCT *****

        #region Server

        void AddServer(Server Network)
        {
            try
            {
                // TODO: Figure out log files checks so we don't get IO exception
                // -Is StreamWriter really the best way to do this? 
                // -Need to do non-locking IO like mIRC.
                // -Change default Capacity value to something like 10
                LogFiles.Add(Network.URI, new Dictionary<string,TextWriter>());
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("TextLogger Broken");
            }
        }
        
        void RemoveServer(Server Network)
        {
            if (NetworkExists(Network))
            {
                LogFiles[Network.URI].Clear();
                LogFiles.Remove(Network.URI);
            }
        }

        #endregion

        #region Channel

        void AddChannel(Channel Chan)
        {
            if (!NetworkExists(Chan.Server) && !ChannelExists(Chan))
            {
                // TODO: Text writer
                LogFiles[Chan.Server.URI].Add(Chan.Name, null);
            }
        }

        void RemoveChannel(Channel Chan)
        {
            if (NetworkExists(Chan.Server))
            {
                LogFiles[Chan.Server.URI].Remove(Chan.Name);
            }
        }

        #endregion

        #region Person

        void AddPerson(Server Network, User Person)
        {
            if (NetworkExists(Network) && !PersonExists(Network, Person))
            {
                // TODO: Text writer
                LogFiles[Network.URI].Add(Person.Nick, null);
            }
        }

        void RemovePerson(Server Network, User Person)
        {
            if (NetworkExists(Network))
            {
                LogFiles[Network.URI].Remove(Person.Nick);
            }
        }

        #endregion

        #endregion
    }
}
