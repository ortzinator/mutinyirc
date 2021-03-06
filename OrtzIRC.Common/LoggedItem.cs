namespace OrtzIRC.Common
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// This class holds a single log file as well as corresponding name and path.
    /// The goal of this class is to abstract away the IO verification/handling made
    /// by the TextLogger class. Consider this class the "Physical" component of the logger.
    /// </summary>
    public class LoggedItem
    {
        // The log directory (This string is simply used to avoid concatenating multiple
        // strings whenever we new the FileHandle member.
        public static string basedir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "OrtzIRC\\logs"); //hack

        // The path of the file we're logging to
        public string LogPath { get; private set; }
        // The name of the loggable item (#luahelp, Ortzinator, etc)
        public string Name { get; private set; }

        // Error handling:

        // Whether the loggable is in working order
        public bool Failed { get; private set; }
        // The last IOException thrown. Implemented, but
        // requires UI tweaking.
        public string LastError { get; private set; }

        // The file we're writing to
        private FileStream fileHandle;

        /// <summary>
        /// Creates a new loggable item in the specified folder underneath the ./logs
        /// directory.
        /// </summary>
        /// <param name="name">The name of the log file, without extension.</param>
        /// <param name="folder">The folder to create the file in. It should NOT start with a \, nor end with one.</param>
        public LoggedItem(string name, string folder)
        {
            Name = name;
            LogPath = Path.Combine(Path.Combine(basedir, folder), name + ".log");
            Open();
        }

        /// <summary>
        /// Opens the stream for writing, making sure the proper directory structure is in place.
        /// </summary>
        public void Open()
        {
            CreateIfNotExists(Path.GetDirectoryName(LogPath));
            fileHandle = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        /// <summary>
        /// Closes the stream for writing.
        /// </summary>
        public void Close()
        {
            fileHandle.Close();
            fileHandle = null;
        }

        /// <summary>
        /// Writes text to the stream
        /// </summary>
        /// <param name="text">The String object to write to the file.</param>
        public void Write(string text)
        {
            // Check if stream is working
            if (!Failed)
            {
                // Convert data
                byte[] data = text.GetBytes();

                // Try to write to the stream once
                Failed = !TryWrite(data);
            }
        }

        /// <summary>
        /// Attempts to write to the stream
        /// </summary>
        /// <param name="data">The data to write</param>
        /// <returns>The success of the operation</returns>
        private bool TryWrite(byte[] data)
        {
            try
            {
                // Make sure the necessary directories are in place
                CreateIfNotExists(Path.GetDirectoryName(LogPath));

                // Attempt writing the data
                fileHandle.Write(data, 0, data.Length);
                fileHandle.Flush();

                return true; // Success
            }
            catch (IOException ex)
            {
                // Set error state
                LastError = ex.ToString();
                return false; // Failure
            }
        }

        // Utility

        private static void CreateIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
