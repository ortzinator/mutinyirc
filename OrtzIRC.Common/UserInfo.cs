using FlamingIRC;

namespace OrtzIRC.Common
{
    public class UserInfo
    {
        /// <summary>The user's nickname.</summary>
        public string Nick { get; set; }

        /// <summary>The user's "real name", immediately before the @</summary>
        public string RealName { get; set; }

        /// <summary>The user's fully qualified host name</summary>
        public string HostMask { get; set; }

        /// <summary>The user's username on the local machine</summary>
        public string UserName { get; set; }

        /// <summary> Nickname plus mode symbol prefix </summary>
        public string NamesLiteral { get; private set; }

        /// <summary> The channel mode symbol prefix from NAMES</summary>
        public char Prefix { get; private set; }

        public static UserInfo FromUser(User user)
        {
            UserInfo u = new UserInfo
            {
                Nick = user.Nick,
                RealName = user.RealName,
                HostMask = user.HostMask,
                UserName = user.UserName,
                NamesLiteral = user.NamesLiteral,
                Prefix = user.Prefix
            };
            return u;
        }
    }
}
