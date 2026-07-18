using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MutinyIRC.PluginFramework;

/// <summary>
/// Provides functions for examining assemblies.
/// </summary>
public static class AssemblyExaminer
{
    /// <summary>
    /// Examines the given assembly for MutinyIRC commands, yielding a <see cref="CommandInfo"/>
    /// for each public, non-abstract, <see cref="PluginAttribute"/>-decorated
    /// <see cref="ICommand"/> implementation. Command descriptions come from
    /// <see cref="PluginAttribute.Description"/>.
    /// </summary>
    /// <param name="asm">The assembly to examine.</param>
    /// <returns>A lazily-enumerated collection of command metadata.</returns>
    public static IEnumerable<CommandInfo> ExamineAssembly(Assembly asm)
    {
        var query = asm.GetTypes().Where(o => o.IsPublic)
            .Where(o => o.IsClass)
            .Where(o => (o.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
            .Where(o => o.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0)
            .Where(o => o.GetInterfaces().Contains(typeof(ICommand)));

        foreach (Type type in query)
        {
            var attr = ((PluginAttribute[])type.GetCustomAttributes(
                typeof(PluginAttribute), false))[0];

            yield return new CommandInfo(asm.Location, type.FullName,
                attr.Name ?? type.Name, typeof(ICommand), attr.Description);
        }
    }
}
