//Courtesy of Godeke from Stack Overflow http://stackoverflow.com/questions/3095573
namespace VisualStudioLockWorkaround
{
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            string file = args[0];
            string fileName = Path.GetFileName(file);
            string directory = Path.GetDirectoryName(args[0]);
            if (!Directory.Exists(directory)) //If we don't have a folder, nothing to do.
            {
                Console.WriteLine(String.Format("Folder {0} not found. Exiting.", directory));
                return;
            }
            if (!File.Exists(file)) //If the offending executable is missing, no reason to remove the locked files.
            {
                Console.WriteLine(String.Format("File {0} not found. Exiting.", file));
                return;
            }
            foreach (string lockedFile in Directory.GetFiles(directory, "*.locked"))
            {
                try //We know IOExceptions will occur due to the locking bug.
                {
                    File.Delete(lockedFile);
                }
                catch (IOException)
                {
                    //Nothing to do, just absorbing the IO error.
                }
                catch (UnauthorizedAccessException)
                {
                    //Nothing to do, just absorbing the IO error.
                }
            }

            //Rename the executable to a .locked
            File.Move(file, Path.Combine(directory, String.Format("{0}{1:ddmmyyhhmmss}.locked", fileName, DateTime.Now)));
        }
    }
}
