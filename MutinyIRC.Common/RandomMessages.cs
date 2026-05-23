using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MutinyIRC.Common
{
    /// <summary>
    /// Generator for random messages of any type (quit, part).
    /// </summary>
    public static class RandomMessages
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "MutinyIRC", "random-messages.json");

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private static Dictionary<string, List<string>> _messagesStore = new();
        private static readonly Random Rand = new();

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
            if (_messagesStore.TryGetValue(messageType, out var list) && !list.Contains(message))
            {
                list.Add(message);
            }
        }

        /// <summary>
        /// Removes a message from a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <param name="message">The message to remove</param>
        public static void RemoveMessage(string messageType, string message)
        {
            if (_messagesStore.TryGetValue(messageType, out var list))
            {
                list.Remove(message);
            }
        }

        /// <summary>
        /// Retrieves a random message from a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <returns>The message, or null if no messages of that type have been entered yet.</returns>
        public static string GetMessage(string messageType)
        {
            if (_messagesStore.TryGetValue(messageType, out var list) && list.Count != 0)
            {
                return list[Rand.Next(0, list.Count)];
            }

            return null;
        }

        /// <summary>
        /// Loads the messages from disk. No-op if the file does not exist yet.
        /// </summary>
        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return;

                using FileStream stream = File.OpenRead(FilePath);
                _messagesStore =
                    JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stream)
                    ?? new Dictionary<string, List<string>>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not load random messages: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the messages to disk.
        /// </summary>
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                using FileStream stream = File.Create(FilePath);
                JsonSerializer.Serialize(stream, _messagesStore, SerializerOptions);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not save random messages: {ex.Message}");
            }
        }
    }
}
