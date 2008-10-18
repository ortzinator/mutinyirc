namespace OrtzIRC.Plugins
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class PluginManager
    {
        private static List<Command> commands;
        private static List<Plugin> plugins;

        private static PluginManager _instance;

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                    return null;
                
                return _instance;
            }
        }

        private PluginManager()
        {
            commands = new List<Command>();

        }

        public static void LoadPlugins()
        {
            if (_instance == null)
                _instance = new PluginManager();

            FindPlugins();
        }

        /// <summary>
        /// Searches the plugins directory for assemblies
        /// </summary>
        private static void FindPlugins()
        {
            Console.WriteLine("Loading plugins...");
            Assembly dll;

            string[] files = Directory.GetFileSystemEntries(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OrtzIRC/Plugins"), "*.dll");

            foreach (string file in files)
            {
                try
                {
                    dll = Assembly.LoadFrom(file);
                    ExamineAssembly(dll);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not load " + file);
                }
            }
        }

        /// <summary>
        /// Examine assembly for OrtzIRC plugins and commands
        /// </summary>
        /// <param name="asm">The assembly to examine</param>
        private static void ExamineAssembly(Assembly asm)
        {
            Type[] types = asm.GetTypes();
            object[] attributes;
            Command newCommand;

            foreach (Type type in types)
            {
                if (type.IsPublic)
                {
                    if ((type.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
                    {
                        attributes = type.GetCustomAttributes(typeof(CommandAttribute), false);
                        if (attributes.Length > 0)
                        {
                            newCommand = new Command(asm.Location, type.FullName);

                            commands.Add(newCommand);
                            Console.WriteLine("Added plugin at " + asm.Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse a command sent from a channel window
        /// </summary>
        /// <param name="channel">The channel object</param>
        /// <param name="line">The entire command line sent</param>
        public void ParseCommand(Channel channel, string line)
        {

        }

        /// <summary>
        /// Parse a command sent from a server window
        /// </summary>
        /// <param name="server">The server object</param>
        /// <param name="line">The entire command line sent</param>
        public void ParseCommand(Server server, string line)
        {
            if (server.Connection.Connected)
            {

            }
        }
    }
}
