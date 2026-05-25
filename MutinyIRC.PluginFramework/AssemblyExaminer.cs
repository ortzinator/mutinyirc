using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using MutinyIRC.Common;

namespace MutinyIRC.PluginFramework
{
    /// <summary>
    /// Provides functions for examining assemblies.
    /// </summary>
    public static class AssemblyExaminer
    {
        /// <summary>
        /// Examines the given assembly for MutinyIRC plugins, yielding a <see cref="CommandInfo"/>
        /// for each <see cref="ICommand"/> implementation and a <see cref="PluginInfo"/> for any
        /// other <see cref="IPlugin"/>. Command descriptions come from
        /// <see cref="PluginAttribute.Description"/>, falling back to the sibling XML doc file
        /// loaded once per assembly.
        /// </summary>
        /// <param name="asm">The assembly to examine.</param>
        /// <returns>A lazily-enumerated collection of plugin metadata.</returns>
        public static IEnumerable<PluginInfo> ExamineAssembly(Assembly asm)
        {
            var query = asm.GetTypes().Where(o => o.IsPublic)
                .Where(o => o.IsClass)
                .Where(o => (o.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
                .Where(o => o.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0)
                .Where(o => o.GetInterfaces().Contains(typeof(IPlugin)));

            XmlDocsParser docs = LoadXmlDocs(asm);

            foreach (Type type in query)
            {
                if (type.GetInterface(typeof(ICommand).FullName) != null)
                {
                    var attr = ((PluginAttribute[])type.GetCustomAttributes(
                        typeof(PluginAttribute), false))[0];
                    string description = attr.Description
                        ?? (docs == null ? null : SafeGetTypeSummary(docs, type, asm));

                    yield return new CommandInfo(asm.Location, type.FullName,
                        attr.Name ?? type.Name, typeof(ICommand), description);
                }
                else
                {
                    yield return new PluginInfo(asm.Location, type.Name, typeof(IPlugin));
                }
            }
        }

        /// <summary>
        /// Looks for a sibling <c>.xml</c> doc file next to the assembly and returns a parser for
        /// it, or <c>null</c> if the file is missing or fails to load. The outcome is traced under
        /// <see cref="TraceCategories.PluginSystem"/> so plugin authors can tell whether their docs
        /// were picked up.
        /// </summary>
        private static XmlDocsParser LoadXmlDocs(Assembly asm)
        {
            string docPath = asm.Location.Remove(asm.Location.Length - 4, 4) + ".xml";

            if (!File.Exists(docPath))
            {
                //Would be useful for plugin devs to know
                Trace.WriteLine($"XML docs not found for the assembly: {asm}",
                    TraceCategories.PluginSystem);
                return null;
            }

            Trace.WriteLine($"XML docs found for the assembly: {asm}",
                TraceCategories.PluginSystem);
            try
            {
                return new XmlDocsParser(docPath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to load XML docs for {asm}: {ex}",
                    TraceCategories.PluginSystem);
                return null;
            }
        }

        /// <summary>
        /// Returns the XML <c>&lt;summary&gt;</c> for <paramref name="type"/>, swallowing any
        /// parser exception so a malformed doc file cannot break plugin discovery. Returns
        /// <c>null</c> when the summary is missing or the lookup throws.
        /// </summary>
        private static string SafeGetTypeSummary(XmlDocsParser docs, Type type, Assembly asm)
        {
            try
            {
                return docs.GetTypeSummary(type);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to read XML summary for {type.FullName} in {asm}: {ex}",
                    TraceCategories.PluginSystem);
                return null;
            }
        }
    }
}