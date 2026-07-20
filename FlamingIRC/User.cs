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

using System;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// Represents a user in a single channel
/// </summary>
public class User : IComparable<User>
{
    // Active channel status symbols held by this user (e.g. '@' and '+' simultaneously).
    private readonly List<char> _statuses = new List<char>();

    // Status symbols from highest rank to lowest. Used to pick the symbol shown in nick lists.
    private static readonly char[] StatusRank = { '~', '&', '@', '%', '+' };

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

    public static User Empty => new User();

    /// <summary>The user's fully qualified host name</summary>
    public string HostMask { get; set; } = string.Empty;

    /// <summary> Nickname plus mode symbol prefix </summary>
    public string NamesLiteral => Prefix != '\0' ? Prefix + Nick : Nick;

    /// <summary>The user's nickname.</summary>
    public string Nick { get; set; } = string.Empty;

    /// <summary>
    /// The highest-ranked channel status symbol the user currently holds (e.g. '@' for a user
    /// who is both op and voiced), or '\0' if the user holds no status.
    /// </summary>
    /// <remarks>
    /// Setting replaces all statuses with the single given symbol ('\0' clears all). To track
    /// op and voice independently use <see cref="AddStatus"/> / <see cref="RemoveStatus"/>.
    /// </remarks>
    public char Prefix
    {
        get
        {
            foreach (char symbol in StatusRank)
                if (_statuses.Contains(symbol))
                    return symbol;
            return '\0';
        }
        set
        {
            if (value != '\0' && !UserModeValidator.IsValid(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            _statuses.Clear();
            if (value != '\0')
                _statuses.Add(value);
        }
    }

    /// <summary>Returns true if the user currently holds the given status symbol.</summary>
    public bool HasStatus(char symbol) => _statuses.Contains(symbol);

    /// <summary>Grants the user a channel status symbol (e.g. '@' for op) if not already held.</summary>
    public void AddStatus(char symbol)
    {
        if (!UserModeValidator.IsValid(symbol))
        {
            throw new ArgumentOutOfRangeException("symbol");
        }
        if (!_statuses.Contains(symbol))
            _statuses.Add(symbol);
    }

    /// <summary>Revokes a channel status symbol from the user, if held.</summary>
    public void RemoveStatus(char symbol) => _statuses.Remove(symbol);

    /// <summary>The user's "real name", immediately before the @</summary>
    public string RealName { get; set; } = string.Empty;
    /// <summary>The user's username on the local machine</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// Takes a nick string from a NAMES and parses it as a User object
    /// </summary>
    public static User FromNames(string nick)
    {
        if (nick == string.Empty)
            return null;

        var user = new User();

        int i = 0;
        while (i < nick.Length && UserModeValidator.IsValid(nick[i]))
        {
            user.AddStatus(nick[i]);
            i++;
        }
        user.Nick = nick.Substring(i);

        return user;
    }

    public int CompareTo(User other) => NamesLiteral.CompareTo(other.NamesLiteral);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(User) && Equals((User)obj);
    }

    public bool Equals(User other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Equals(other.Nick, Nick);
    }

    public override int GetHashCode() => Nick != null ? Nick.GetHashCode() : 0;
    public override string ToString() => NamesLiteral;
}
