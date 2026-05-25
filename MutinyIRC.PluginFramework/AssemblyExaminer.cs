using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        /// <see cref="PluginAttribute.Description"/>.
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

            foreach (Type type in query)
            {
                if (type.GetInterface(typeof(ICommand).FullName) != null)
                {
                    var attr = ((PluginAttribute[])type.GetCustomAttributes(
                        typeof(PluginAttribute), false))[0];

                    yield return new CommandInfo(asm.Location, type.FullName,
                        attr.Name ?? type.Name, typeof(ICommand), attr.Description);
                }
                else
                {
                    yield return new PluginInfo(asm.Location, type.Name, typeof(IPlugin));
                }
            }
        }
    }
}