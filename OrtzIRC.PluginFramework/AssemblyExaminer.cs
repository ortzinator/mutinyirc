namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public static class AssemblyExaminer
    {
        /// <summary>
        /// Examine assembly for OrtzIRC plugins and commands
        /// </summary>
        /// <param name="asm">The assembly to examine</param>
        public static IEnumerable<PluginInfo> ExamineAssembly(Assembly asm)
        {
            var query = asm.GetTypes().Where(o => o is IPlugin)
                .Where(o => o.IsPublic)
                .Where(o => o.IsClass)
                .Where(o => (o.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
                .Where(o => o.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0);

            foreach (Type type in query)
            {
                if (type is ICommand)
                {
                    List<CommandAutocompleteAttribute> aCompletes = new List<CommandAutocompleteAttribute>();

                    foreach (MethodInfo info in type.GetMethods())
                    {
                        object[] att = info.GetCustomAttributes(typeof(CommandAutocompleteAttribute), false);
                        if (att.Length > 0)
                        {
                            aCompletes.AddRange(att as IEnumerable<CommandAutocompleteAttribute>);
                        }
                    }

                    if (aCompletes.Count < 1)
                    {
                        continue;
                    }

                    yield return new CommandInfo(asm.Location, type.FullName, type, aCompletes);
                }
                else
                {
                    yield return new PluginInfo(asm.Location, type.FullName, type);
                }

            }
        }
    }
}