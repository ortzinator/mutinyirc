using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlamingIRC
{
    public class UserModeValidator
    {
        private static readonly char[] _modes = new char[] { '@', '+', '%', '&', '~' };

        public static char[] Modes
        {
            get { return _modes; }
        }

        public static bool IsValid(char value)
        {
            return _modes.Contains(value);
        }
    }
}
