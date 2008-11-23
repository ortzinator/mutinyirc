namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OrtzIRC.Common;
    using OrtzIRC.Properties;

    /// <summary>
    /// Manages plugins and commands.
    /// </summary>
    public sealed class PluginManager
    {
        private static List<CommandInfo> commands;

        public static PluginManager Instance { get; private set; }

        public string UserPluginPath { get; private set; }

        private PluginManager(string pluginPath)
        {
            UserPluginPath = pluginPath;
            commands = new List<CommandInfo>();
        }

        /// <summary>
        /// Instantiates the PluginManager and loads any plugins found.
        /// </summary>
        /// <remarks>Must be called first</remarks>
        internal static void LoadPlugins()
        {
            if (Instance == null)
                Instance = new PluginManager();

            FindPlugins();
        }

        /// <summary>
        /// Searches the plugins directory for assemblies
        /// </summary>
        private static void FindPlugins()
        {
            Trace.WriteLine("Loading Plug-ins", TraceCategories.PluginSystem);

            Assembly dll;

            string[] files = Directory.GetFileSystemEntries(this.UserPluingPath, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    dll = Assembly.LoadFrom(file);
                    ExamineAssembly(dll);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not load " + file + Environment.NewLine + ex.ToString(), TraceCategories.PluginSystem);
                }
            }
        }

        /// <summary>
        /// Examine assembly for OrtzIRC plugins and commands
        /// </summary>
        /// <param name="asm">The assembly to examine</param>
        private static void ExamineAssembly(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                if ((type.IsPublic) && ((type.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(CommandAttribute), false);
                    if (attributes.Length > 0)
                    {
                        CommandInfo newCommand = new CommandInfo(asm.Location, type.FullName);

                        commands.Add(newCommand);
                        Trace.WriteLine("Added plugin at " + asm.Location);
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
            if (server.IsConnected)
            {

            }
        }
    }
}
