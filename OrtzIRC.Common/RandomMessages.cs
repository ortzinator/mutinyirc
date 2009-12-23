namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Generator for random messages of any type (quit, part).
    /// </summary>
    public static class RandomMessages
    {
        private static SerializableDictionary<string, List<string>> _messagesStore;
        private static Random _rand;

        /// <summary>
        /// Adds a message type to the list
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.) used later to fetch individual random messages</param>
        /// <returns>True if the type registered correctly, false if it was already registered before</returns>
        public static bool RegisterMessageType(string messageType)
        {
            if (_messagesStore.ContainsKey(messageType))
                return false;

            _messagesStore.Add(messageType, new List<string>());
            return true;
        }

        /// <summary>
        /// Removes a message type from the list. This also removes any messages of that type.
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <returns></returns>
        public static bool UnregisterMessageType(string messageType)
        {
            return _messagesStore.Remove(messageType);
        }

        /// <summary>
        /// Adds a message to a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <param name="message">The message</param>
        public static void AddMessage(string messageType, string message)
        {
            if (_messagesStore.ContainsKey(messageType) && !_messagesStore[messageType].Contains(message))
            {
                _messagesStore[messageType].Add(message);
            }
        }

        /// <summary>
        /// Removes a message from a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <param name="message">The message to remove</param>
        public static void RemoveMessage(string messageType, string message)
        {
            if (_messagesStore.ContainsKey(messageType))
            {
                _messagesStore[messageType].Remove(message);
            }
        }

        /// <summary>
        /// Retreives a random message from a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <returns>The message, or null if no messages of that type have been entered yet.</returns>
        public static string GetMessage(string messageType)
        {
            if (_messagesStore.ContainsKey(messageType))
            {
                List<string> listRef = _messagesStore[messageType];

                if (listRef.Count != 0)
                    return listRef[_rand.Next(0, listRef.Count)];
            }

            return null;
        }

        /// <summary>
        /// Loads the settings from XML
        /// </summary>
        public static void Load()
        {
            _rand = new Random();

            // TODO: Real path
            if (File.Exists(Environment.CurrentDirectory + "\\list.xml"))
            {
                XmlSerializer s = new XmlSerializer(typeof(SerializableDictionary<string, List<string>>));
                TextReader r = new StreamReader(Environment.CurrentDirectory + "\\list.xml");

                try
                {
                    _messagesStore = (SerializableDictionary<string, List<string>>)s.Deserialize(r);
                    r.Close();
                }
                catch (Exception ex)
                {
                    // TODO: Let's tell the user about it or something
                    throw;
                }
            }
            else
            {
                _messagesStore = new SerializableDictionary<string, List<string>>();
            }
        }

        /// <summary>
        /// Saves the settings to XML
        /// </summary>
        public static void Save()
        {
            XmlSerializer s = new XmlSerializer(typeof(SerializableDictionary<string, List<string>>));
            TextWriter w = new StreamWriter(Environment.CurrentDirectory + "\\list.xml");

            try
            {
                s.Serialize(w, _messagesStore);
                w.Close();
            }
            catch (Exception ex)
            {
                // TODO: Let's tell the user about it or something
                throw;
            }
        }
    }
}