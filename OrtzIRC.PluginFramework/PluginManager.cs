namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using OrtzIRC.Common;

    /// <summary>
    /// Manages plugins and commands.
    /// </summary>
    public sealed class PluginManager
    {
        private static List<PluginInfo> plugins;
        private static List<CommandInfo> commands;

        public static PluginManager Instance { get; private set; }

        public string UserPluginPath { get; private set; }

        private PluginManager()
        {
            plugins = new List<PluginInfo>();
            commands = new List<CommandInfo>();
        }

        /// <summary>
        /// Instantiates the PluginManager and loads any plugins found.
        /// </summary>
        /// <remarks>Must be called first</remarks>
        internal static void LoadPlugins(string pluginPath)
        {
            if (Instance == null)
                Instance = new PluginManager();

            Instance.UserPluginPath = pluginPath;

            FindPlugins();
        }

        /// <summary>
        /// Searches the plugins directory for assemblies
        /// </summary>
        private static void FindPlugins()
        {
            Trace.WriteLine("Loading Plug-ins", TraceCategories.PluginSystem);

            string[] files = Directory.GetFileSystemEntries(Instance.UserPluginPath, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    foreach (PluginInfo info in AssemblyExaminer.ExamineAssembly(Assembly.LoadFrom(file)))
                    {
                        plugins.Add(info);
                        Trace.WriteLine("Added plugin at " + info.AssemblyPath);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not load " + file + Environment.NewLine + ex.ToString(), TraceCategories.PluginSystem);
                }
            }
        }

        public static ICommand GetCommandInstance(Channel channel, string command, params string[] parameters)
        {
            //TODO
            foreach (CommandInfo c in commands)
            {
                
            }
            return null;
        }

        public static ICommand GetCommandInstance(Server server, string command, params string[] parameters)
        {
            //TODO
            foreach (CommandInfo c in commands)
            {

            }
            return null;
        }

        public static ICommand GetCommandInstance(PrivateMessageSession pmSession, string command, params string[] parameters)
        {
            //TODO
            foreach (CommandInfo c in commands)
            {

            }
            return null;
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
