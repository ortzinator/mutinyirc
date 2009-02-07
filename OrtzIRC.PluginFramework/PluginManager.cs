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
        private static Dictionary<string, CommandInfo> commands;

        public static PluginManager Instance { get; private set; }

        private static string userPluginPath;

        private PluginManager()
        {
            plugins = new List<PluginInfo>();
            commands = new Dictionary<string, CommandInfo>();
        }

        /// <summary>
        /// Instantiates the PluginManager and loads any plugins found.
        /// </summary>
        /// <remarks>Must be called first</remarks>
        public static void LoadPlugins(string pluginPath)
        {
            if (Instance == null)
                Instance = new PluginManager();

            userPluginPath = pluginPath;

            FindPlugins();
        }

        /// <summary>
        /// Searches the plugins directory for assemblies, examines them for plugins and populates
        /// </summary>
        private static void FindPlugins()
        {
            Trace.WriteLine("Loading Plug-ins", TraceCategories.PluginSystem);

            string[] files = Directory.GetFileSystemEntries(userPluginPath, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    foreach (PluginInfo info in AssemblyExaminer.ExamineAssembly(Assembly.LoadFrom(file)))
                    {
                        if (info is CommandInfo)
                        {
                            if (!commands.ContainsKey(info.ClassName))
                            {
                                commands.Add(info.ClassName, info as CommandInfo);
                                Trace.WriteLine("Added command plugin " + info.ClassName + " at " + info.AssemblyPath, TraceCategories.PluginSystem);
                            }
                            else
                            {
                                Trace.WriteLine("Could not load command " + info.ClassName + ". A command by that name already exists.",
                                    TraceCategories.PluginSystem); //Hack: lousy error message :P
                            }
                        }
                        else
                        {
                            plugins.Add(info);
                            Trace.WriteLine("Added plugin " + info.ClassName + " at " + info.AssemblyPath, TraceCategories.PluginSystem);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not load " + file + Environment.NewLine + ex.ToString(), TraceCategories.PluginSystem);
                }
            }
        }

        public static ICommand GetCommandInstance(Channel channel, string command, string[] parameters)
        {
            //TODO
            foreach (KeyValuePair<string, CommandInfo> item in commands)
            {
                if (item.Key == command)
                {
                    CommandInfo cmd = item.Value;

                    foreach (MethodInfo method in cmd.Type.GetMethods())
                    {

                    }
                }
            }
            return null;
        }

        public static ICommand GetCommandInstance(Server server, string command, string[] parameters)
        {
            //TODO
            foreach (KeyValuePair<string, CommandInfo> item in commands)
            {

            }
            return null;
        }

        public static ICommand GetCommandInstance(PrivateMessageSession pmSession, string command, string[] parameters)
        {
            //TODO
            foreach (KeyValuePair<string, CommandInfo> item in commands)
            {

            }
            return null;
        }
    }
}
