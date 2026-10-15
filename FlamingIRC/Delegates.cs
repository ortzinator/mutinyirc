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

namespace FlamingIRC;

/// <summary>
/// The user has been invited to a channel.
/// </summary>
/// <param name="user">Who sent the invite.</param>
/// <param name="channel">The target channel.</param>
/// <seealso cref="Listener.OnInvite"/>
public delegate void InviteEventHandler(User user, string channel);

/// <summary>
/// The full unparsed text messages received from the IRC server. It
/// includes all messages received except for those exchanged during a DCC chat.
/// </summary>
/// <param name="message">The text received.</param>
/// <see cref="Connection.OnRawMessageReceived"/>
public delegate void RawMessageReceivedEventHandler(string message);

/// <summary>
/// The full unparsed text messages sent to the IRC server. It
/// includes all messages sent except for those exchanged during a DCC chat.
/// </summary>
/// <param name="message">The text sent.</param>
/// <see cref="Connection.RawMessageSent"/>
public delegate void RawMessageSentEventHandler(string message);
