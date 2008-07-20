using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    /// <summary>
    /// Represents a user in a single channel
    /// </summary>
    public class Nick : OrtzIRC.IRC.Target
    {
        private string _hostMask;
        private string _name;
        private string _realName;
        private string _userName;
    }
}
