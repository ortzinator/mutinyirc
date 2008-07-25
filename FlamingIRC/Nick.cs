using System;
using System.Collections.Generic;
using System.Text;

namespace FlamingIRC
{
    /// <summary>
    /// Represents a user in a single channel
    /// </summary>
    public class Nick : Target
    {
        private string _hostMask;

        public string HostMask
        {
            get { return _hostMask; }
            set { _hostMask = value; }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _realName;

        public string RealName
        {
            get { return _realName; }
            set { _realName = value; }
        }
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public static Nick Empty
        {
            get
            {
                return new Nick();
            }
        }
    }
}
