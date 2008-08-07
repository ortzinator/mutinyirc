using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    /// <summary>
    /// Represents a user in a single channel
    /// </summary>
    public class Nick : Target
    {
        public string Name { get; set; }
        public string RealName { get; set; }
        public string HostMask { get; set; }
        public string UserName { get; set; }

        public static Nick Empty
        {
            get
            {
                return new Nick();
            }
        }

        public static Nick FromUserInfo(Sharkbite.Irc.UserInfo info)
        {
            var nick = new Nick();
            nick.HostMask = info.Hostname;
            nick.Name = info.Nick;

            return nick;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
