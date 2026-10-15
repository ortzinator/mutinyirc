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
/// Someone has requested a DCC chat session.
/// </summary>
/// <param name="dccUser">The collection of information about the remote user.</param>
/// <see cref="DccListener.OnDccChatRequest"/>
public delegate void DccChatRequestEventHandler(DccUser dccUser);

/// <summary>
/// A DCC chat session has been opened with a remote user.
/// </summary>
/// <param name="session">Which session is the source of the event.</param>
/// <see cref="DccChatSession.OnChatSessionOpened"/>
public delegate void ChatSessionOpenedEventHandler(DccChatSession session);

/// <summary>
/// A DCC chat session has been closed.
/// </summary>
/// <param name="session">Which session is the source of the event.</param>
/// <see cref="DccChatSession.OnChatSessionClosed"/>
public delegate void ChatSessionClosedEventHandler(DccChatSession session);

/// <summary>
/// A DCC chat message was received from the remote user.
/// </summary>
/// <param name="session">Which session is the source of the event.</param>
/// <param name="message">A string message.</param>
/// <see cref="DccChatSession.OnChatMessageReceived"/>
public delegate void ChatMessageReceivedEventHandler(DccChatSession session, string message);

/// <summary>
/// When trying to initiate a DCC chat request the remote user
/// did not respond within the alotted time.
/// </summary>
/// <param name="session">Which session is the source of the event.</param>
/// <see cref="DccChatSession.OnChatRequestTimeout"/>
public delegate void ChatRequestTimeoutEventHandler(DccChatSession session);

/// <summary>
/// Another user has offered to send a file.
/// </summary>
/// <param name="dccUser">The collection of information about the remote user.</param>
/// <param name="fileName">The name of the file to be sent.</param>
/// <param name="size">The size in bytes of the offered file.</param>
/// <param name="turbo">True if the sender will use send-ahead mode.</param>
/// <see cref="DccListener.OnDccSendRequest"/>
public delegate void DccSendRequestEventHandler(DccUser dccUser, string fileName, int size, bool turbo);

/// <summary>
/// There has been no activity in this session for the timeout period. The
/// session is automatically closed and this event is raised.
/// </summary>
/// <param name="session">The session in which the timeout occurred.</param>
/// <see cref="DccFileSession.OnFileTransferTimeout"/>
public delegate void FileTransferTimeoutEventHandler(DccFileSession session);

/// <summary>
/// The file transfer connection has been successfully opened and the data
/// transfer has begun.
/// </summary>
/// <param name="session">The session in which the transfer has started.</param>
/// <see cref="DccFileSession.OnFileTransferStarted"/>
public delegate void FileTransferStartedEventHandler(DccFileSession session);

/// <summary>
/// Something happened to stop the transfer before it was completed. Normally
/// this will be due to one of the sides canceling the transfer.
/// </summary>
/// <param name="session">The session in which the transfer was interrupted.</param>
/// <see cref="DccFileSession.OnFileTransferInterrupted"/>
public delegate void FileTransferInterruptedEventHandler(DccFileSession session);

/// <summary>
/// A file was succefully transfered.
/// </summary>
/// <param name="session">The session in which the transfer was successfully completed.</param>
/// <see cref="DccFileSession.OnFileTransferCompleted"/>
public delegate void FileTransferCompletedEventHandler(DccFileSession session);

/// <summary>
/// Called for each successful data block transfer. This allows the developer
/// to show a transfer progress display of some kind.
/// </summary>
/// <param name="session">The session in which data was transfered.</param>
/// <param name="bytesSent">The number of bytes sent in this block. The DccFileSession
/// contains the cumulative number of bytes sent/received and the total number
/// the will be processed.</param>
/// <see cref="DccFileSession.OnFileTransferProgress"/>
public delegate void FileTransferProgressEventHandler(DccFileSession session, int bytesSent);

/// <summary>
/// A remote user has requested a file. To respond
/// use <see cref="DccFileSession.Send"/> with the relevant information.
/// </summary>
/// <param name="dccUser">The requestor's information.</param>
/// <param name="fileName">The name of the requested file.</param>
/// <param name="turbo">True to use send-ahead mode for transfers.</param>
/// <see cref="DccListener.OnDccGetRequest"/>
public delegate void DccGetRequestEventHandler(DccUser dccUser, string fileName, bool turbo);

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
