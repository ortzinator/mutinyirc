namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using OrtzIRC.Common;
    using System.Linq;

    /// <summary>
    /// Manages plugins and commands.
    /// </summary>
    public sealed class PluginManager
    {
        private static Dictionary<string, CommandInfo> commands;
        private static List<PluginInfo> plugins;

        private static string userPluginPath;

        private PluginManager()
        {
            plugins = new List<PluginInfo>();
            commands = new Dictionary<string, CommandInfo>();
        }

        public static PluginManager Instance { get; private set; }

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
                            if (!commands.ContainsKey(info.FullName))
                            {
                                commands.Add(info.FullName, info as CommandInfo);
                                Trace.WriteLine("Added command plugin " + info.FullName + " at " + info.AssemblyPath, TraceCategories.PluginSystem);
                            }
                            else
                            {
                                Trace.WriteLine("Could not load command " + info.FullName + ". A command by that name already exists.",
                                    TraceCategories.PluginSystem); //Hack: lousy error message :P
                            }
                        }
                        else
                        {
                            plugins.Add(info);
                            Trace.WriteLine("Added plugin " + info.FullName + " at " + info.AssemblyPath, TraceCategories.PluginSystem);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not load " + file + Environment.NewLine + ex, TraceCategories.PluginSystem);
                }
            }
            Trace.WriteLine("Finished loading Plug-ins", TraceCategories.PluginSystem);
        }

        private static ICommand GetCommandInstance(string name)
        {
            //TODO
            foreach (KeyValuePair<string, CommandInfo> item in commands)
            {
                if (item.Value.CommandName == name)
                {
                    return (ICommand)CreateInstance(item.Value);
                }
            }
            return null;
        }

        private static IPlugin CreateInstance(PluginInfo pluginInfo)
        {
            Assembly asm = Assembly.LoadFile(pluginInfo.AssemblyPath);

            return (IPlugin)asm.CreateInstance(pluginInfo.FullName);
        }

        public static CommandResultInfo ExecuteCommand(CommandExecutionInfo info)
        {
            //TODO: This should handle errors
            GetCommandInstance(info.Name).GetType().GetMethods(System.Reflection.BindingFlags.Instance)
            .Where(o => o.Name == "Execute")
            .Where(o => o.GetParameters()[0].ParameterType == typeof(MessageContext));

            return new CommandResultInfo(); //Hack: So it builds
        }
    }
}
