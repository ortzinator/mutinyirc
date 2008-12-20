namespace OrtzIRC.PluginFramework
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CommandAutocompleteAttribute : Attribute
    {
        public string Description { get; private set; }
        public string[] Parameters { get; private set; }

        public CommandAutocompleteAttribute(string description, params string[] parameters)
        {
            Description = description;
            Parameters = parameters;
        }
    }
}
