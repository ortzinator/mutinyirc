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
        public static String Basedir = Environment.CurrentDirectory + '\\' + "logs";

        // The path of the file we're logging to
        public String Path { get; private set; }
        // The directory of that file
        public String Dir  { get; private set; }
        // The name of the loggable item (#luahelp, Ortzinator, etc)
        public String Name { get; private set; }

        // Error handling:

        // Whether the loggable is in working order
        public bool Failed { get; private set; }
        // The last IOException thrown. Implemented, but
        // requires UI tweaking.
        public String LastError { get; private set; }

        // The file we're writing to
        private FileStream FileHandle;

        /// <summary>
        /// Creates a new loggable item in the specified folder underneath the ./logs
        /// directory.
        /// </summary>
        /// <param name="Name">The name of the log file, without extension.</param>
        /// <param name="Folder">The folder to create the file in. It should NOT start with a \, nor end with one.</param>
        public LoggedItem(String Name, String Folder)
        {
            this.Name = Name;
            Dir = Basedir + '\\' + Folder;
            Path = Dir + '\\' + Name + ".log";
            Open();
        }

        /// <summary>
        /// Opens the stream for writing, making sure the proper directory structure is in place.
        /// </summary>
        public void Open()
        {
            CreateIfNotExists(Dir);
            FileHandle = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        /// <summary>
        /// Closes the stream for writing.
        /// </summary>
        public void Close()
        {
            FileHandle.Close();
        }

        /// <summary>
        /// Writes text to the stream
        /// </summary>
        /// <param name="Text">The String object to write to the file.</param>
        public void Write(String Text)
        {
            // Check if stream is working
            if (!Failed)
            {
                // Convert data
                byte[] Data= GetBytes(Text);

                // Try to write to the stream once
                Failed = TryWrite(Data);
            }
        }

        /// <summary>
        /// Attempts to write to the stream
        /// </summary>
        /// <param name="Data">The data to write</param>
        /// <returns>The success of the operation</returns>
        private bool TryWrite(byte[] Data)
        {
            try
            {
                // Make sure the necessary directories are in place
                CreateIfNotExists(Dir);

                // Attempt writing the data
                FileHandle.Write(Data, 0, Data.Length);
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

        private static void CreateIfNotExists(String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // Transform text to bytes for writing to stream
        private static readonly ASCIIEncoding encoding = new ASCIIEncoding();
        private static byte[] GetBytes(String Str)
        {
            return (encoding.GetBytes(Str));
        }
    }
}
