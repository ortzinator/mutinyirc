using System;
using System.Collections.Generic;
using System.Text;

namespace FlamingIRC
{
    public abstract class Target
    {
        public abstract override string ToString();
    }

    /// <summary>
    /// A channel topic
    /// </summary>
    public class Topic : ClientMessage
    {
        private Nick _setter;

        public Nick Setter
        {
            get { return _setter; }
            set { _setter = value; }
        }
        private string _text;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }

    /// <summary>
    /// Abstract class representing any message recieved from the server
    /// </summary>
    public abstract class ClientMessage
    {
        private Target _target;

        public Target Target
        {
            get { return _target; }
            set { _target = value; }
        }
        private DateTime _time;

        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
    }

    /// <summary>
    /// IRC Error
    /// </summary>
    public class IRCError : ClientMessage
    {
    }

    public class Message : ClientMessage
    {
        private string _msg;

        public string Msg
        {
            get { return _msg; }
            set { _msg = value; }
        }
        private Target _sender;

        public Target Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
    }

    /// <summary>
    /// CTCP Message
    /// </summary>
    public class CTCP : Message
    {
        private string _parameters;

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }
    }

    public class PrivMsg : Message
    {
    }
}
