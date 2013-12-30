/*
 * FlamingIRC IRC library
 * Copyright (C) 2008 Brian Ortiz & Max Schmeling <https://github.com/ortzinator/mutinyirc>
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
    public class User : IComparable<User>
    {
        private string _hostMask = String.Empty;

        private string _nick = String.Empty;

        private char _prefix;

        private string _realName = String.Empty;

        private string _userName = String.Empty;

        public User() { }

        /// <summary>
        /// This constructors assumes nick contains no prefix (as in the rest of the library).
        /// Params: nick!user@host
        /// </summary>
        /// <param name="nick">nick</param>
        /// <param name="user">user</param>
        /// <param name="host">host</param>
        public User(string nick, string user, string host)
        {
            Nick = nick;
            UserName = user;
            HostMask = host;
            Prefix = '\0';
        }

        public static User Empty
        {
            get { return new User(); }
        }

        /// <summary>The user's fully qualified host name</summary>
        public string HostMask
        {
            get { return _hostMask; }
            set { _hostMask = value; }
        }

        /// <summary> Nickname plus mode symbol prefix </summary>
        public string NamesLiteral { get { return Prefix != '\0' ? Prefix + Nick : Nick; } }

        /// <summary>The user's nickname.</summary>
        public string Nick
        {
            get { return _nick; }
            set { _nick = value; }
        }

        /// <summary> The channel mode symbol prefix from NAMES</summary>
        public char Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                if (value != '\0' && !UserModeValidator.IsValid(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _prefix = value;
            }
        }

        /// <summary>The user's "real name", immediately before the @</summary>
        public string RealName
        {
            get { return _realName; }
            set { _realName = value; }
        }
        /// <summary>The user's username on the local machine</summary>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        /// <summary>
        ///   Takes a nick string from a NAMES and parses it as a User object
        /// </summary>
        public static User FromNames(string nick)
        {
            if (nick == String.Empty)
                return null;

            char mode = nick[0];
            var user = new User();

            if (UserModeValidator.IsValid(mode))
            {
                user.Nick = nick.Substring(1);
                user.Prefix = mode;
            }
            else
            {
                user.Nick = nick;
            }

            return user;
        }

        public int CompareTo(User other)
        {
            return NamesLiteral.CompareTo(other.NamesLiteral);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(User) && Equals((User)obj);
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(other.Nick, Nick);
        }

        public override int GetHashCode()
        {
            return (Nick != null ? Nick.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return NamesLiteral;
        }
    }
}