namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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
            var query = asm.GetTypes().Where(o => o is ICommand)
                .Where(o => o.IsPublic)
                .Where(o => o.IsClass)
                .Where(o => (o.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract)
                .Where(o => o.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0)
                .Where(o => o.GetInterfaces().Contains<Type>(typeof(IPlugin)));

            foreach (Type type in query)
            {
                yield return new PluginInfo(asm.Location, type.FullName, type);
            }
        }
    }
}