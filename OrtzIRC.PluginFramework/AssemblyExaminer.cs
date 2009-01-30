namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.IO;
    using System.Diagnostics;
    using OrtzIRC.Common;

    /// <summary>
    /// Provides functions for examining assemblies.
    /// </summary>
    public static class AssemblyExaminer
    {
        /// <summary>
        /// Examine assembly for OrtzIRC plugins
        /// </summary>
        /// <param name="asm">The assembly to examine</param>
        /// <returns>A collection of PluginInfo</returns>
        public static IEnumerable<PluginInfo> ExamineAssembly(Assembly asm)
        {
            var query = asm.GetTypes().Where(o => o.IsPublic)
                .Where(o => o.IsClass)
                .Where(o => (o.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
                .Where(o => o.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0)
                .Where(o => o.GetInterfaces().Contains<Type>(typeof(IPlugin)));

            foreach (Type type in query)
            {
                if (type.GetInterface(typeof(ICommand).FullName) != null)
                {
                    string docPath = asm.Location.Remove(asm.Location.Length - 4, 4) + ".xml";

                    if (!File.Exists(docPath))
                    {
                        //Would be useful for plugin devs to know
                        Trace.WriteLine("XML docs not found for the assembly: " + asm.ToString(), TraceCategories.PluginSystem);
                        continue;
                    }

                    //TODO: stuff for commands
                    yield return new CommandInfo(asm.Location, type.Name, typeof(ICommand));
                }
                else
                {
                    yield return new PluginInfo(asm.Location, type.Name, typeof(IPlugin));
                }
            }
        }
    }
}