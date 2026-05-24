namespace MutinyIRC.PluginFramework
{
    using MutinyIRC.Common;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Manages plugins and commands.
    /// </summary>
    public sealed class PluginManager
    {
        internal Dictionary<string, CommandInfo> _commands;
        private List<PluginInfo> _plugins;

        public PluginManager()
        {
            _plugins = new List<PluginInfo>();
            _commands = new Dictionary<string, CommandInfo>();
        }

        /// <summary>
        /// Instantiates the PluginManager and loads any plugins found.
        /// </summary>
        /// <remarks>
        /// Must be called first
        /// </remarks>
        public void LoadPlugins(string pluginPath)
        {
            FindPlugins(pluginPath);
        }

        /// <summary>
        /// Searches the plugins directory for assemblies, examines them for plugins and populates
        /// </summary>
        private void FindPlugins(string path)
        {
            Trace.WriteLine($"Loading Plug-ins ({path})", TraceCategories.PluginSystem);

            if (!Directory.Exists(path))
            {
                Trace.WriteLine($"Plugin directory not found: {path}", TraceCategories.PluginSystem);
                return;
            }

            string[] files = Directory.GetFileSystemEntries(path, "*.dll");

            var tempCommands = new Dictionary<string, CommandInfo>();

            foreach (string file in files)
            {
                try
                {
                    foreach (PluginInfo info in AssemblyExaminer.ExamineAssembly(Assembly.LoadFrom(file)))
                    {
                        if (info is CommandInfo)
                        {
                            if (!_commands.ContainsKey(info.FullName))
                            {
                                tempCommands.Add(info.FullName, info as CommandInfo);
                                Trace.WriteLine($"Added command plugin {info.FullName} at {info.AssemblyPath}", TraceCategories.PluginSystem);
                            }
                            else
                            {
                                Trace.WriteLine($"Could not load command {info.FullName} at {info.AssemblyPath}. A command by that name already exists at {_commands[info.FullName].AssemblyPath}.",
                                    TraceCategories.PluginSystem);
                            }
                        }
                        else
                        {
                            _plugins.Add(info);
                            Trace.WriteLine($"Added plugin {info.FullName} at {info.AssemblyPath}", TraceCategories.PluginSystem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Could not load: {file} ({ex})", TraceCategories.PluginSystem);
                }
            }

            if (tempCommands.Count == 0)
                Trace.WriteLine($"No plugins found in directory: {path}", TraceCategories.PluginSystem);

            foreach (var pair in tempCommands)
            {
                _commands.Add(pair.Key, pair.Value);
            }

            Trace.WriteLine("Finished loading Plug-ins", TraceCategories.PluginSystem);
        }

        /// <summary>
        /// Gets command instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when name argument is null.
        /// </exception>
        /// <param name="name">The command name.</param>
        /// <returns>The command instance.</returns>
        private ICommand GetCommandInstance(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            foreach (var item in _commands)
            {
                if (item.Value.CommandName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return (ICommand)CreateInstance(item.Value);
            }
            Trace.WriteLine($"No command called {name.ToUpper()} found", TraceCategories.PluginSystem);
            return null;
        }

        private IPlugin CreateInstance(PluginInfo pluginInfo)
        {
            try
            {
                Assembly asm = FindLoadedAssembly(pluginInfo.AssemblyPath)
                    ?? Assembly.LoadFile(pluginInfo.AssemblyPath);
                var instance = asm.CreateInstance(pluginInfo.FullName);
                if (instance == null)
                    Trace.WriteLine($"CreateInstance returned null for {pluginInfo.FullName} in {pluginInfo.AssemblyPath}", TraceCategories.PluginSystem);
                return (IPlugin)instance;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to instantiate {pluginInfo.FullName}: {ex}", TraceCategories.PluginSystem);
                return null;
            }
        }

        private static Assembly FindLoadedAssembly(string path)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;
                if (string.Equals(asm.Location, path, StringComparison.OrdinalIgnoreCase))
                    return asm;
            }
            return null;
        }

        public CommandResultInfo ExecuteCommand(CommandExecutionInfo commandInput)
        {
            //TODO: This should handle errors
            //TODO: Pretty complex, maybe could use some commenting

            Trace.WriteLine($"Command: /{commandInput.Name} [{string.Join(", ", commandInput.ParameterList)}]", TraceCategories.PluginSystem);

            ICommand commandInstance = GetCommandInstance(commandInput.Name);
            if (commandInstance == null)
                return CommandResultInfo.Fail($"{commandInput.Name.ToUpper()} is an invalid command");

            MethodInfo[] commandMethods = commandInstance.GetType().GetMethods()
                .Where(o => o.Name == "Execute")
                .Where(o => o.GetParameters()[0].ParameterType.BaseType == typeof(MessageContext)).ToArray();

            //Sort descending by number of parameters so the more "specific" methods are given priority
            Array.Sort(commandMethods,
                       (m1, m2) => -m1.GetParameters().Length.CompareTo(m2.GetParameters().Length));

            foreach (MethodInfo methodInfo in commandMethods)
            {
                ParameterInfo[] methodParameters = methodInfo.GetParameters();
                int parameterCount = methodParameters.Length - 1;

                //Loop throught the method's parameters
                for (int p = 0; p < methodParameters.Length; p++)
                {
                    ParameterInfo methodParameter = methodParameters[p];

                    if (commandInput.ParameterList.Count < parameterCount)
                        break;

                    if (p == 0)
                    {
                        //First parameter must match the current context type
                        if (methodParameter.ParameterType != commandInput.Context.GetType())
                            break;

                        //Handle parameterless command
                        if (commandInput.ParameterList.Count == 0)
                        {
                            if (parameterCount != 0)
                                break;

                            commandInput.ParameterList.Insert(0, commandInput.Context);
                            try
                            {
                                return (CommandResultInfo)methodInfo.Invoke(commandInstance, commandInput.ParameterList.ToArray());
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"Command '{commandInput.Name}' threw an exception: {ex}", TraceCategories.PluginSystem);
                                return CommandResultInfo.Fail($"{commandInput.Name.ToUpper()} failed with an error");
                            }
                        }
                        continue;
                    }

                    // If it's a channel, convert the string into a ChannelInfo object
                    if (FlamingIRC.Rfc2812Util.IsValidChannelName(commandInput.ParameterList[p - 1] as string))
                        commandInput.ParameterList[p - 1] = new ChannelInfo(commandInput.ParameterList[p - 1] as string);

                    var sp = commandInput.ParameterList[p - 1] as string;
                    if (sp != null && sp.StartsWith("-")) // Check for switches
                    {
                        sp = sp.Remove(0, 1);
                        commandInput.ParameterList[p - 1] = sp.ToCharArray();
                    }

                    if (methodParameter.ParameterType != commandInput.ParameterList[p - 1].GetType())
                        break; //Parameter mismatch. Break parameter loop and go the the next method

                    if (p != parameterCount) continue; //If this isn't the last parameter then keep looping

                    //Checks for an "open-ended" string parameter.
                    if (methodParameter.ParameterType == typeof(string))
                    {
                        bool allStrings = true;
                        var openString = new System.Text.StringBuilder();

                        int numberOpenEnded = 0;
                        for (int k = p - 1; k < commandInput.ParameterList.Count; k++)
                        {
                            if (commandInput.ParameterList[k].GetType() != typeof(string))
                            {
                                allStrings = false;
                                break;
                            }

                            openString.Append(commandInput.ParameterList[k] + " ");
                            numberOpenEnded++;
                        }

                        if (allStrings && p != commandInput.ParameterList.Count)
                        {
                            openString.Remove(openString.Length - 1, 1);
                            commandInput.ParameterList.RemoveRange(p - 1, numberOpenEnded);
                            commandInput.ParameterList.Add(openString.ToString());
                        }
                    }
                    commandInput.ParameterList.Insert(0, commandInput.Context);
                    try
                    {
                        return (CommandResultInfo)methodInfo.Invoke(commandInstance, commandInput.ParameterList.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Command '{commandInput.Name}' threw an exception: {ex}", TraceCategories.PluginSystem);
                        return CommandResultInfo.Fail($"{commandInput.Name.ToUpper()} failed with an error");
                    }
                }
            }
            Trace.WriteLine($"No matching Execute() overload for command '{commandInput.Name}' with context {commandInput.Context.GetType().Name} and {commandInput.ParameterList.Count} parameter(s)",
                TraceCategories.PluginSystem);
            return null;
        }

        public CommandExecutionInfo ParseCommand(MessageContext context, string line)
        {
            if (line == null) throw new ArgumentNullException("line");

            if (line.StartsWith("/"))
            {
                string[] exploded = line.Split(new char[] { ' ' });
                string name = exploded[0].TrimStart('/');
                string[] parameters = new string[exploded.Length - 1];
                Array.Copy(exploded, 1, parameters, 0, exploded.Length - 1); //Removing the first element

                return new CommandExecutionInfo
                {
                    Context = context,
                    Name = name,
                    ParameterList = new List<object>(parameters)
                };
            }
            else
            {
                string[] parameters = line.Split(new char[] { ' ' });
                return new CommandExecutionInfo
                {
                    Context = context,
                    Name = "say",
                    ParameterList = new List<object>(parameters)
                };
            }
        }
    }
}