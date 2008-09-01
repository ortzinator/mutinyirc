namespace OrtzIRC
{
    using System;
    using OrtzIRC.Common;

    /// <summary>
    /// Represents a user in a single channel
    /// </summary>
    public class Nick : Target
    {
        public string Name { get; set; }
        public string RealName { get; set; }
        public string HostMask { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// Nickname plus mode symbol prefix
        /// </summary>
        public string NamesLiteral { get; private set; }
        public char Prefix { get; private set; }

        public static Nick Empty
        {
            get { return new Nick(); }
        }

        public static Nick FromUserInfo(Sharkbite.Irc.UserInfo info)
        {
            var nick = new Nick();
            nick.HostMask = info.Hostname;
            nick.Name = info.Nick;

            return nick;
        }

        public static Nick FromNames(string nick)
        {
            var tempNick = new Nick();
            char firstChar = Char.Parse(nick.Substring(0, 1));
            char[] modes = new char[] { Char.Parse("@"), Char.Parse("+"), Char.Parse("%"), Char.Parse("&"), Char.Parse("~") };

            foreach (char c in modes)
            {
                if (firstChar == c)
                {
                    tempNick.Prefix = Char.Parse(nick.Substring(0, 1));
                    tempNick.Name = nick.Substring(1);
                    tempNick.NamesLiteral = nick;
                    return tempNick;
                }
            }

            tempNick.Name = nick;
            tempNick.NamesLiteral = nick;
            return tempNick;
        }

        public override string ToString()
        {
            return "{0} ({1})".With(this.UserName, this.RealName); // todo - replace. this probably isn't how we want it formatted.
        }
    }
}
