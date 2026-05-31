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

        public PluginManager()
        {
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
                Trace.WriteLine($"Plugin directory not found: {path}",
                    TraceCategories.PluginSystem);
                return;
            }

            string[] files = Directory.GetFileSystemEntries(path, "*.dll");

            var tempCommands = new Dictionary<string, CommandInfo>();

            foreach (string file in files)
            {
                try
                {
                    foreach (CommandInfo info in AssemblyExaminer.ExamineAssembly(
                                 Assembly.LoadFrom(file)))
                    {
                        if (!_commands.ContainsKey(info.FullName))
                        {
                            tempCommands.Add(info.FullName, info);
                            Trace.WriteLine(
                                $"Added command plugin {info.FullName} at {info.AssemblyPath}",
                                TraceCategories.PluginSystem);
                        }
                        else
                        {
                            Trace.WriteLine(
                                $"Could not load command {info.FullName} at {info.AssemblyPath}. A command by that name already exists at {_commands[info.FullName].AssemblyPath}.",
                                TraceCategories.PluginSystem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Could not load: {file} ({ex})", TraceCategories.PluginSystem);
                }
            }

            if (tempCommands.Count == 0)
                Trace.WriteLine($"No plugins found in directory: {path}",
                    TraceCategories.PluginSystem);

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
            Trace.WriteLine($"No command called {name.ToUpper()} found",
                TraceCategories.PluginSystem);
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
                    Trace.WriteLine(
                        $"CreateInstance returned null for {pluginInfo.FullName} in {pluginInfo.AssemblyPath}",
                        TraceCategories.PluginSystem);
                return (IPlugin)instance;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to instantiate {pluginInfo.FullName}: {ex}",
                    TraceCategories.PluginSystem);
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

        /// <summary>
        /// Resolves the command named in <paramref name="commandInput"/> and dispatches to the best-matching
        /// <c>Execute</c> overload on the command instance.
        /// </summary>
        /// <param name="commandInput">
        /// The parsed command: its name, the user-supplied argument list, and the originating
        /// <see cref="MessageContext"/>. The argument list is not mutated.
        /// </param>
        /// <returns>
        /// The result returned by the dispatched <c>Execute</c> overload;
        /// <see cref="CommandResultInfo.Fail(string)"/> if the command name is unknown or the dispatched
        /// overload threw; or <c>null</c> if the command exists but no overload matches the supplied
        /// context type and argument shape.
        /// </returns>
        /// <remarks>
        /// Overloads are tried most-specific first (highest parameter count). Each candidate gets a fresh
        /// copy of <see cref="CommandExecutionInfo.ParameterList"/>, so coercions applied while trying one
        /// overload (e.g. promoting a string to <see cref="ChannelInfo"/> or <see cref="char"/>[]) never
        /// leak into the next attempt.
        /// </remarks>
        public CommandResultInfo ExecuteCommand(CommandExecutionInfo commandInput)
        {
            Trace.WriteLine(
                $"Command: /{commandInput.Name} [{string.Join(", ", commandInput.ParameterList)}]",
                TraceCategories.PluginSystem);

            ICommand commandInstance = GetCommandInstance(commandInput.Name);
            if (commandInstance == null)
                return CommandResultInfo.Fail(
                    $"{commandInput.Name.ToUpper()} is an invalid command");

            foreach (MethodInfo overload in GetExecuteOverloads(commandInstance))
            {
                CommandResultInfo result =
                    TryInvokeOverload(overload, commandInstance, commandInput);
                if (result != null) return result;
            }

            Trace.WriteLine(
                $"No matching Execute() overload for command '{commandInput.Name}' with context {commandInput.Context.GetType().Name} and {commandInput.ParameterList.Count} parameter(s)",
                TraceCategories.PluginSystem);
            return null;
        }

        /// <summary>
        /// Returns the command's <c>Execute</c> overloads whose first parameter derives from
        /// <see cref="MessageContext"/>, sorted most-specific first (descending by parameter count).
        /// </summary>
        private static MethodInfo[] GetExecuteOverloads(ICommand command)
        {
            return command.GetType().GetMethods()
                .Where(m => m.Name == "Execute")
                .Where(m => m.GetParameters()[0].ParameterType.BaseType == typeof(MessageContext))
                .OrderByDescending(m => m.GetParameters().Length)
                .ToArray();
        }

        /// <summary>
        /// Attempts to match <paramref name="input"/> against a single <c>Execute</c> overload and invoke it.
        /// </summary>
        /// <param name="method">The candidate <c>Execute</c> overload.</param>
        /// <param name="instance">The command instance to invoke against.</param>
        /// <param name="input">The parsed command input. Not mutated.</param>
        /// <returns>
        /// The invocation result, or <c>null</c> if <paramref name="method"/> does not match the input
        /// (wrong arity, context type mismatch, or post-coercion argument type mismatch).
        /// </returns>
        private static CommandResultInfo TryInvokeOverload(MethodInfo method, ICommand instance,
                                                           CommandExecutionInfo input)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            int userParamCount = methodParams.Length - 1;

            if (input.ParameterList.Count < userParamCount) return null;
            if (methodParams[0].ParameterType != input.Context.GetType()) return null;

            // Work on a copy so coercions from a failed match don't leak into the next overload.
            var args = new List<object>(input.ParameterList);

            if (userParamCount == 0)
            {
                if (args.Count != 0) return null;
                args.Insert(0, input.Context);
                return InvokeSafely(method, instance, args.ToArray(), input.Name);
            }

            for (int i = 0; i < userParamCount; i++)
            {
                args[i] = CoerceArgument(args[i]);
                if (methodParams[i + 1].ParameterType != args[i].GetType()) return null;
            }

            if (methodParams[userParamCount].ParameterType == typeof(string))
                CollapseTrailingStrings(args, userParamCount - 1);

            args.Insert(0, input.Context);
            return InvokeSafely(method, instance, args.ToArray(), input.Name);
        }

        /// <summary>
        /// Promotes a bare string argument to a richer type: <see cref="ChannelInfo"/> for channel-name
        /// strings, or <see cref="char"/>[] for <c>-flag</c> switches. Non-string arguments are returned unchanged.
        /// </summary>
        private static object CoerceArgument(object arg)
        {
            if (arg is string s)
            {
                if (FlamingIRC.Rfc2812Util.IsValidChannelName(s))
                    return new ChannelInfo(s);
                if (s.StartsWith("-"))
                    return s.Substring(1).ToCharArray();
            }
            return arg;
        }

        /// <summary>
        /// Folds the trailing string arguments at and after <paramref name="startIndex"/> in
        /// <paramref name="list"/> into a single space-joined string at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="list">The argument list to modify in place.</param>
        /// <param name="startIndex">The index of the first argument to fold.</param>
        /// <remarks>
        /// The list is only modified when there are trailing extras
        /// (<c>list.Count &gt; startIndex + 1</c>) and every argument from
        /// <paramref name="startIndex"/> onward is a <see cref="string"/>; otherwise it is left unchanged.
        /// </remarks>
        private static void CollapseTrailingStrings(List<object> list, int startIndex)
        {
            if (startIndex + 1 >= list.Count) return;
            for (int k = startIndex; k < list.Count; k++)
            {
                if (list[k] is not string) return;
            }

            var joined = string.Join(" ", list.Skip(startIndex).Cast<string>());
            list.RemoveRange(startIndex, list.Count - startIndex);
            list.Add(joined);
        }

        private static CommandResultInfo InvokeSafely(MethodInfo method, ICommand instance,
                                                      object[] args, string commandName)
        {
            try
            {
                return (CommandResultInfo)method.Invoke(instance, args);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Command '{commandName}' threw an exception: {ex}",
                    TraceCategories.PluginSystem);
                return CommandResultInfo.Fail($"{commandName.ToUpper()} failed with an error");
            }
        }

        public CommandExecutionInfo ParseCommand(MessageContext context, string line)
        {
            if (line == null) throw new ArgumentNullException("line");

            if (line.StartsWith("/"))
            {
                string[] exploded = line.Split(new char[] { ' ' });
                string name = exploded[0].TrimStart('/');
                string[] parameters = new string[exploded.Length - 1];
                Array.Copy(exploded, 1, parameters, 0,
                    exploded.Length - 1); //Removing the first element

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
