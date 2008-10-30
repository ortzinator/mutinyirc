namespace OrtzIRC.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandSetAttribute : Attribute
    {
        public string CommandName { get; private set; }

        public CommandSetAttribute() { }

        public CommandSetAttribute(string name)
        {
            this.CommandName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute()
        {
            //intentionally left blank
        }
    }

    [Flags]
    public enum CommandContext : int
    {
        Channel = 1,
        Server = 2,
        PrivateMessage = 4,
        All = 7,
    }
}
