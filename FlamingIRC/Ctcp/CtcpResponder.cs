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
using System.Text;

namespace FlamingIRC;

/// <summary>
/// A class which automatically responds to CTCP queries. The
/// replies it sends are configurable by the client.
/// </summary>
public sealed class CtcpResponder
{
    private readonly Connection _connection;
    private long _nextTime;
    private string _fingerMessage;

    /// <summary>
    /// Create an instance and register handlers for
    /// CTCP events. The Connection's CtcpEnable property must
    /// be set to true or the connection will not send CTCP events
    /// to this responder.
    /// </summary>
    /// <param name="connection">The containing connection.</param>
    public CtcpResponder(Connection connection)
    {
        _connection = connection;
        _nextTime = DateTime.Now.ToFileTime();
        //Wait at least 2 second in between automatic CTCP responses
        ResponseDelay = 2000;
        //Send back user nick by default for finger requests.
        UserResponse = "Thresher CTCP Responder";
        _fingerMessage = UserResponse;
        VersionResponse = "Thresher IRC library 1.1";
        SourceResponse = "http://thresher.sourceforge.net";
        ClientInfoResponse = "This client supports: UserInfo, Finger, Version, Source, Ping, Time and ClientInfo";
        if (connection.EnableCtcp)
        {
            connection.CtcpListener.OnCtcpRequest += OnCtcpRequest;
            connection.CtcpListener.OnCtcpPingRequest += OnCtcpPingRequest;
        }
    }

    /// <summary>
    /// How long the responder should wait before
    /// replying to a query. Queries coming before this
    /// time has passed will be droppped.
    /// </summary>
    /// <value>The delay in milliseconds. The default is 2000 (2 seconds).</value>
    public double ResponseDelay { get; set; }
    /// <summary>
    /// Finger responses normally consist of a message
    /// and the idle time.
    /// </summary>
    /// <value>The Idle time will be automatically appended
    /// to the finger response. This default to the User message.</value>
    public string FingerResponse
    {
        get => _fingerMessage + " Idle time " + FormatIdleTime() + "."; set => _fingerMessage = value;
    }
    /// <summary>
    /// A message about the user.
    /// </summary>
    /// <value>Any string which does not exceed the IRC max length.
    /// This defaults to "Thresher Auto-Responder".</value>
    public string UserResponse { get; set; }
    /// <summary>
    /// The version of the client software.
    /// </summary>
    /// <value>This defaults to "Thresher IRC library 1.0".</value>
    public string VersionResponse { get; set; }
    /// <summary>
    /// Tell others what CTCP commands this client supports.
    /// </summary>
    /// <value>By default it sends a list of all the CTCP commands.</value>
    public string ClientInfoResponse { get; set; }
    /// <summary>
    /// Where to get this client.
    /// </summary>
    /// <value>This can be a complex set of FTP instructions or just a
    /// URL to the client's homepage.</value>
    public string SourceResponse { get; set; }

    /// <summary>
    /// For a TimeSpan to show only hours,minutes, and seconds.
    /// </summary>
    /// <returns>A beautified TimeSpan</returns>
    private string FormatIdleTime()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(_connection.IdleTime.Hours + " Hours, ");
        builder.Append(_connection.IdleTime.Minutes + " Minutes, ");
        builder.Append(_connection.IdleTime.Seconds + " Seconds");
        return builder.ToString();
    }
    /// <summary>
    /// Format the current date into date, time, and time zone. Used
    /// by Time replies.
    /// </summary>
    /// <returns>A beautified DateTime</returns>
    private string FormatDateTime()
    {
        DateTime time = DateTime.Now;
        StringBuilder builder = new StringBuilder();
        builder.Append(time.ToLongDateString() + " ");
        builder.Append(time.ToLongTimeString() + " ");
        builder.Append("(" + TimeZoneInfo.Local.StandardName + ")");
        return builder.ToString();
    }
    /// <summary>
    /// Create the next time period and adding the correct number
    /// of ticks. No Ctcp replies will be sent if the current time is not later
    /// than this value.
    /// </summary>
    private void UpdateTime() => _nextTime = DateTime.Now.ToFileTime() +
        (long)(ResponseDelay * TimeSpan.TicksPerMillisecond);
    private void OnCtcpRequest(object sender, CtcpRequestEventArgs e)
    {
        string command = e.Command;
        User who = e.User;
        if (DateTime.Now.ToFileTime() > _nextTime)
        {
            switch (command)
            {
                case CtcpUtil.Finger:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, _fingerMessage + " Idle time: " + FormatIdleTime());
                    break;
                case CtcpUtil.Time:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, FormatDateTime());
                    break;
                case CtcpUtil.User:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, UserResponse);
                    break;
                case CtcpUtil.Version:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, VersionResponse);
                    break;
                case CtcpUtil.Source:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, SourceResponse);
                    break;
                case CtcpUtil.ClientInfo:
                    _connection.CtcpSender.CtcpReply(command, who.Nick, ClientInfoResponse);
                    break;
                default:
                    string error = command + " is not a supported Ctcp query.";
                    _connection.CtcpSender.CtcpReply(command, who.Nick, error);
                    break;
            }
            UpdateTime();
        }
    }
    private void OnCtcpPingRequest(object sender, CtcpPingEventArgs e)
        => _connection.CtcpSender.CtcpPingReply(e.User.Nick, e.Timestamp);

    /// <summary>
    /// Stop listening to the CtcpListener.
    /// </summary>
    internal void Disable()
    {
        _connection.CtcpListener.OnCtcpRequest -= OnCtcpRequest;
        _connection.CtcpListener.OnCtcpPingRequest -= OnCtcpPingRequest;
    }

}
