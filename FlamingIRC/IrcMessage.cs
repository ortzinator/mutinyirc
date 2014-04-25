using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlamingIRC
{
    public class IrcMessage
    {
        private string _command;
        private string _from;
        private string _message;
        private ReplyCode _replyCode;

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public ReplyCode ReplyCode
        {
            get { return _replyCode; }
            set { _replyCode = value; }
        }
    }
}
