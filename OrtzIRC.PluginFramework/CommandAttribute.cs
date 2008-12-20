namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Description { get; private set; }
        public string[] Parameters { get; private set; }

        public CommandAttribute(string description, params string[] parameters)
        {
            Description = description;
            Parameters = parameters;
        }
    }
}
