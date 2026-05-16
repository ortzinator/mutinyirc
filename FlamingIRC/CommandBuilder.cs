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
    using System.Text;

    /// <summary>
    /// CommandBuilder provides the support methods needed
    /// by its subclasses to build correctly formatted messages for
    /// the IRC server. It is never itself instantiated.
    /// </summary>
    public abstract class CommandBuilder
    {
        // Buffer to hold commands 

        //Containing connection instance

        internal const char SPACE = ' ';
        internal const string SPACE_COLON = " :";
        internal const int MAX_COMMAND_SIZE = 512;
        internal const char CtcpQuote = '\u0001';

        internal CommandBuilder(Connection connection)
        {
            Connection = connection;
            Buffer = new StringBuilder(MAX_COMMAND_SIZE);
        }

        internal Connection Connection { get; }
        internal StringBuilder Buffer { get; }

        /// <summary>
        /// This methods actually sends the notice and privmsg commands.
        /// It assumes that the message has already been broken up
        /// and has a valid target.
        /// </summary>
        internal void SendMessage(string type, string target, string message)
        {
            Buffer.Append(type);
            Buffer.Append(SPACE);
            Buffer.Append(target);
            Buffer.Append(SPACE_COLON);
            Buffer.Append(message);
            Connection.SendCommand(Buffer);
        }
        /// <summary>
        /// Clear the contents of the string buffer.
        /// </summary>
        internal void ClearBuffer()
        {
            Buffer.Remove(0, Buffer.Length);
        }
        /// <summary>
        /// Break up a large message into smaller pieces that will fit within the IRC
        /// max message size.
        /// </summary>
        /// <param name="message">The text to be broken up</param>
        /// <param name="maxSize">The largest size a piece can be</param>
        /// <returns>A string array holding the correctly sized messages.</returns>
        internal string[] BreakUpMessage(string message, int maxSize)
        {
            int pieces = (int)Math.Ceiling(message.Length / (float)maxSize);
            string[] parts = new string[pieces];
            for (int i = 0; i < pieces; i++)
            {
                int start = i * maxSize;
                parts[i] = i == pieces - 1 ? message.Substring(start) : message.Substring(start, maxSize);
            }
            return parts;
        }
    }
}
