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
        public static PluginInfo ExamineAssembly(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                if ((type.IsPublic) && ((type.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(PluginAttribute), false);
                    //TODO: Should check the interface!
                    if (attributes.Length > 0)
                    {
                        return new PluginInfo(asm.Location, type.FullName, type);
                    }
                }
            }
            return null;
        }
    }
}