/*
 * FlamingIRC IRC library
 * Copyright (C) 2008 Brian Ortiz & Max Schmeling <http://code.google.com/p/ortzirc>
 * 
 * Based on code copyright (C) 2002 Aaron Hunter <thresher@sharkbite.org>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * 
 * See the gpl.txt file located in the top-level-directory of
 * the archive of this library for complete text of license.
*/

namespace FlamingIRC
{
    using System;

    /// <summary>
    /// Represents a user in a single channel
    /// </summary>
    public class User
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

        public static User Empty
        {
            get { return new User(); }
        }

        public User() { }

        public User(string nick, string name, string host)
        {
            Nick = nick;
            UserName = name;
            HostMask = host;
        }

        /// <summary>
        ///   Takes a nick string from a NAMES and parses it as a User object
        /// </summary>
        public static User FromNames(string nick)
        {
            if (nick == String.Empty)
                return null;

            var tempNick = new User();
            char firstChar = Char.Parse(nick.Substring(0, 1));
            char[] modes = new char[] { Char.Parse("@"), Char.Parse("+"), Char.Parse("%"), Char.Parse("&"), Char.Parse("~") };

            foreach (char c in modes)
            {
                if (firstChar == c)
                {
                    tempNick.Prefix = Char.Parse(nick.Substring(0, 1));
                    tempNick.Nick = nick.Substring(1);
                    tempNick.NamesLiteral = nick;
                    return tempNick;
                }
            }

            tempNick.Nick = nick;
            tempNick.NamesLiteral = nick;
            return tempNick;
        }

        public override string ToString()
        {
            return this.NamesLiteral;
        }
    }
}
