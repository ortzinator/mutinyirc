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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 * 
 * See the gpl.txt file located in the top-level-directory of
 * the archive of this library for complete text of license.
*/

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace FlamingIRC;

/// <summary>
/// This class is used to send CTCP specific events. Once registered with this object
/// the client can receive notification of all CTCP requests, pings, and replies. Instances
/// of this class are not created directly but are retrieved as a property from a
/// <see cref="Connection"/> object.
/// </summary>
/// <remarks>All CTCP messages come in Request/Reply pairs. Each event
/// signals either the Request or the Response. All CTCP queries (with the exception of
/// CTCP Ping) are very similiar so they are all handled by the same set of events.</remarks>
///<example><code>
/// //Create a Connection object which will support CTCP (second bool param).
/// Connection connection = new Connection( args, true, false );
/// //Register a delegate on this CtcpListener.
/// connection.CtcpListener.OnCtcpRequest += MyOnCtcpRequest;
/// //If the Connection was created without CTCP support then this property will return null.
/// //However, CTCP can be turned on and off dynamically. To enable it at a later time call:
/// connection.EnableCtcp = true;
/// //Now you can register listeners as above.
/// //Setting EnableCtcp to false will delete the instance of CtcpListener and no more
/// //CTCP events will be raised.
///</code></example>
public class CtcpListener
{
    /// <summary>
    /// A user replied to a CTCP query that this client sent.
    /// </summary>
    public event EventHandler<CtcpReplyEventArgs> OnCtcpReply;
    /// <summary>
    /// A user sent a CTCP query to this client.
    /// </summary>
    public event EventHandler<CtcpRequestEventArgs> OnCtcpRequest;
    /// <summary>
    /// A user replied to a CTCP PING that this client sent.
    /// </summary>
    public event EventHandler<CtcpPingEventArgs> OnCtcpPingReply;
    /// <summary>
    /// A user sent a CTCP PING to this client.
    /// </summary>
    public event EventHandler<CtcpPingEventArgs> OnCtcpPingRequest;

    private static readonly Regex CtcpRegex;
    private static readonly string CtcpTypes;
    private readonly Connection _connection;
    private const int Name = 0;
    private const int Command = 1;
    private const int Text = 2;

    static CtcpListener()
    {
        CtcpTypes = "(FINGER|USERINFO|VERSION|SOURCE|CLIENTINFO|ERRMSG|PING|TIME)";
        CtcpRegex = new Regex(":([^ ]+) [A-Z]+ [^:]+:\u0001" + CtcpTypes + "([^\u0001]*)\u0001", RegexOptions.Compiled | RegexOptions.Singleline);
    }

    /// <summary>
    /// Create a new listener using a specific connection.
    /// </summary>
    /// <param name="connection">The connection to the IRC server.</param>
    internal CtcpListener(Connection connection) => _connection = connection;

    private bool IsReply(string[] tokens)
    {
        if (tokens[Text].Length == 0)
        {
            return false;
        }
        return true;
    }

    private static string[] TokenizeMessage(string message)
    {
        try
        {
            Match match = CtcpRegex.Match(message);
            return new string[] { match.Groups[1].ToString(), match.Groups[2].ToString(), match.Groups[3].ToString().Trim() };
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal void Parse(string line)
    {
        string[] ctcpTokens = TokenizeMessage(line);
        if (ctcpTokens != null)
        {
            if (ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture) == CtcpUtil.Ping)
            {
                if (_connection.CtcpSender.IsMyRequest(ctcpTokens[Text]))
                {
                    _connection.CtcpSender.ReplyReceived(ctcpTokens[Text]);
                    OnCtcpPingReply?.Invoke(this, new CtcpPingEventArgs(
                        Rfc2812Util.UserFromString(ctcpTokens[Name]), ctcpTokens[Text]));
                }
                else
                {
                    //Ignore PING's with now parameters
                    if (ctcpTokens[Text] != null && ctcpTokens[Text].TrimEnd().Length != 0)
                    {
                        OnCtcpPingRequest?.Invoke(this, new CtcpPingEventArgs(
                            Rfc2812Util.UserFromString(ctcpTokens[Name]), ctcpTokens[Text]));
                    }
                }
            }
            else
            {
                if (IsReply(ctcpTokens))
                {
                    OnCtcpReply?.Invoke(this, new CtcpReplyEventArgs(
                        ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture),
                        Rfc2812Util.UserFromString(ctcpTokens[Name]), ctcpTokens[Text]));
                }
                else
                {
                    OnCtcpRequest?.Invoke(this, new CtcpRequestEventArgs(
                        ctcpTokens[Command].ToUpper(CultureInfo.CurrentCulture),
                        Rfc2812Util.UserFromString(ctcpTokens[Name])));
                }
            }
        }
        else
        {
            _connection.Listener.Error(ReplyCode.UnparseableMessage, line);
            Debug.WriteLineIf(CtcpUtil.CtcpTrace.TraceWarning, "Unknown CTCP command '" + line + "' recieved by CtcpListener");
        }
    }

    /// <summary>
    /// Test if the message contains CTCP commands.
    /// </summary>
    /// <param name="message">The raw message from the IRC server</param>
    /// <returns>True if this is a Ctcp request or reply.</returns>
    public static bool IsCtcpMessage(string message) => CtcpRegex.IsMatch(message);

}
