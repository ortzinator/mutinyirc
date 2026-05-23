using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MutinyIRC.Common
{
    /// <summary>
    /// Generator for random messages of any type (quit, part).
    /// </summary>
    public class RandomMessages
    {
        private static readonly string DefaultFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "MutinyIRC", "random-messages.json");

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private static RandomMessages _instance;

        /// <summary>
        /// Process-wide instance backed by the default file path. Lazily loads on first access.
        /// </summary>
        public static RandomMessages Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new RandomMessages(DefaultFilePath);
                _instance.Load();
                return _instance;
            }
        }

        private readonly string _filePath;
        private readonly Random _rand;
        private Dictionary<string, List<string>> _messagesStore = new();

        /// <summary>
        /// Creates a new instance. Tests should use this directly; app code should use <see cref="Instance"/>.
        /// </summary>
        /// <param name="filePath">Path to the JSON file used by Load/Save.</param>
        /// <param name="rand">Random source. Pass a seeded Random for deterministic tests.</param>
        public RandomMessages(string filePath, Random rand = null)
        {
            _filePath = filePath;
            _rand = rand ?? new Random();
        }

        /// <summary>
        /// Adds a message type to the list
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.) used later to fetch individual random messages</param>
        /// <returns>True if the type registered correctly, false if it was already registered before</returns>
        public bool RegisterMessageType(string messageType)
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
        public bool UnregisterMessageType(string messageType)
        {
            return _messagesStore.Remove(messageType);
        }

        /// <summary>
        /// Adds a message to a certain type
        /// </summary>
        /// <param name="messageType">Type of message (quit, part, etc.)</param>
        /// <param name="message">The message</param>
        public void AddMessage(string messageType, string message)
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
        public void RemoveMessage(string messageType, string message)
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
        public string GetMessage(string messageType)
        {
            if (_messagesStore.TryGetValue(messageType, out var list) && list.Count != 0)
            {
                return list[_rand.Next(0, list.Count)];
            }

            return null;
        }

        /// <summary>
        /// Loads the messages from disk. No-op if the file does not exist yet.
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return;

                using FileStream stream = File.OpenRead(_filePath);
                _messagesStore = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stream)
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
        public void Save()
        {
            string tempPath = _filePath + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

                using (FileStream stream = File.Create(tempPath))
                {
                    JsonSerializer.Serialize(stream, _messagesStore, SerializerOptions);
                }

                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not save random messages: {ex.Message}");
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch (IOException) { }
            }
        }
    }
}