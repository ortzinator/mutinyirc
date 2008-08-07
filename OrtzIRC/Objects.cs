using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
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
        public Nick Setter { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// Abstract class representing any message recieved from the server
    /// </summary>
    public abstract class ClientMessage
    {
        public Target Target { get; set; }
        public DateTime Time { get; set; }
    }

    /// <summary>
    /// IRC Error
    /// </summary>
    public class IRCError : ClientMessage
    {
    }

    public class Message : ClientMessage
    {
        public string Msg { get; set; }
        public Target Sender { get; set; }

        public Message() { }

        public Message(string text)
        {
            Msg = text;
        }
    }

    /// <summary>
    /// CTCP Message
    /// </summary>
    public class CTCP : Message
    {
        public string Parameters { get; set; }
    }

    /// <summary>
    /// A user to user private message
    /// </summary>
    public class PrivMsg : Message
    {
        public new Nick Sender { get; set; }
    }
}
