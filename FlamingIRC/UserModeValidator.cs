using System;
using System.Linq;

namespace FlamingIRC;

public class UserModeValidator
{
    public static char[] Modes { get; } = new char[] { '@', '+', '%', '&', '~' };

    public static bool IsValid(char value)
    {
        return Modes.Contains(value);
    }
}
