﻿namespace OrtzIRC.PluginFramework
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
                .Where(o => o.GetInterfaces().Contains(typeof(IPlugin)));

            foreach (Type type in query)
            {
                if (type.GetInterface(typeof(ICommand).FullName) != null)
                {
                    string docPath = asm.Location.Remove(asm.Location.Length - 4, 4) + ".xml";
                    XmlDocsParser parser = new XmlDocsParser(docPath);

                    if (File.Exists(docPath))
                    {
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            string summary = parser.GetMethodSummary(type, method);
                            if (summary != null)
                                Console.WriteLine(summary);
                        }
                    }
                    else
                    {
                        //Would be useful for plugin devs to know
                        Trace.WriteLine("XML docs not found for the assembly: " + asm, TraceCategories.PluginSystem);
                    }

                    string name = ((PluginAttribute[])type.GetCustomAttributes(typeof(PluginAttribute), false))[0].Name;

                    //TODO: stuff for commands
                    if (name == null)
                    {
                        yield return new CommandInfo(asm.Location, type.FullName, type.Name, typeof(ICommand));
                    }
                    else
                    {
                        yield return new CommandInfo(asm.Location, type.FullName, name, typeof(ICommand));
                    }
                }
                else
                {
                    yield return new PluginInfo(asm.Location, type.Name, typeof(IPlugin));
                }
            }
        }
    }
}