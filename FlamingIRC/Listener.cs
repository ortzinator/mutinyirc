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
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// This class parses messages received from the IRC server and
    /// raises the appropriate event. 
    /// </summary>
    public sealed class Listener
    {
        /// <summary>
        /// Messages that are not handled by other events and are not errors.
        /// </summary>
        public event EventHandler<ReplyEventArgs> OnReply;
        /// <summary>
        /// Error messages from the IRC server.
        /// </summary>
        public event EventHandler<ErrorMessageEventArgs> OnError;
        /// <summary>
        ///A <see cref="Sender.PrivateNotice"/> or <see cref="Sender.PrivateMessage"/> message was sent to someone who is away.
        /// </summary>
        public event EventHandler<AwayEventArgs> OnAway;
        /// <summary>
        /// An <see cref="Sender.Invite"/> message was successfully sent to another user. 
        /// </summary>
        public event EventHandler<InviteSentEventArgs> OnInviteSent;
        /// <summary>
        /// The user tried to change his nick but it failed.
        /// </summary>
        public event EventHandler<NickErrorEventArgs> OnNickError;
        /// <summary>
        /// A server keep-alive message.
        /// </summary>
        public event PingEventHandler OnPing;
        /// <summary>
        /// Keep track of activity
        /// </summary>
        public event EventHandler OnAnything;
        /// <summary>
        /// Connection with the IRC server is open and registered.
        /// </summary>
        public event EventHandler OnRegistered;
        /// <summary>
        /// This connection is about to be closed. 
        /// </summary>
        //public event DisconnectingEventHandler OnDisconnecting;
        /// <summary>
        /// This connection has been closed. 
        /// </summary>
        //public event EventHandler OnDisconnected;
        /// <summary>
        /// A Notice type message was sent to a channel.
        /// </summary>
        public event EventHandler<UserChannelMessageEventArgs> OnPublicNotice;
        /// <summary>
        /// A private Notice type message was sent to the user.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnPrivateNotice;
        /// <summary>
        /// Someone has joined a channel.
        /// </summary>
        public event JoinEventHandler OnJoin;
        /// <summary>
        /// A public message was sent to a channel.
        /// </summary>
        public event EventHandler<UserChannelMessageEventArgs> OnPublic;
        /// <summary>
        /// An action message was sent to a channel.
        /// </summary>
        public event EventHandler<UserChannelMessageEventArgs> OnAction;
        /// <summary>
        /// A private action message was sent to the user.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnPrivateAction;
        /// <summary>
        /// A user changed his nickname.
        /// </summary>
        public event EventHandler<NickChangeEventArgs> OnNick;
        /// <summary>
        /// A private message was sent to the user.
        /// </summary>
        public event EventHandler<UserMessageEventArgs> OnPrivate;
        /// <summary>
        /// A channel's topic has changed.
        /// </summary>
        public event EventHandler<UserChannelMessageEventArgs> OnTopicChanged;
        /// <summary>
        /// The response to a <see cref="Sender.RequestTopic"/> command.
        /// </summary>
        public event TopicRequestEventHandler OnRecieveTopic;
        /// <summary>
        /// Someone has left a channel. 
        /// </summary>
        public event PartEventHandler OnPart;
        /// <summary>
        /// Someone has quit IRC.
        /// </summary>
        public event QuitEventHandler OnQuit;
        /// <summary>
        /// The user has been invited to a channel.
        /// </summary>
        public event InviteEventHandler OnInvite;
        /// <summary>
        /// Someone has been kicked from a channel. 
        /// </summary>
        public event KickEventHandler OnKick;
        /// <summary>
        /// The response to a <see cref="Sender.Names"/> request.
        /// </summary>
        public event EventHandler<NamesEventArgs> OnNames;
        /// <summary>
        /// The response to a <see cref="Sender.List"/> request.
        /// </summary>
        public event ListEventHandler OnList;
        /// <summary>
        /// The response to a <see cref="Sender.Ison"/> request.
        /// </summary>
        public event IsonEventHandler OnIson;
        /// <summary>
        /// The response to a <see cref="Sender.Who"/> request.
        /// </summary>
        public event WhoEventHandler OnWho;
        /// <summary>
        /// The response to a <see cref="Sender.Whois"/> request.
        /// </summary>
        public event WhoisEventHandler OnWhois;
        /// <summary>
        /// The response to a <see cref="Sender.Whowas"/> request.
        /// </summary>
        public event WhowasEventHandler OnWhowas;
        /// <summary>
        /// Someone's user mode has changed.
        /// </summary>
        public event EventHandler<UserModeChangeEventArgs> OnUserModeChange;
        /// <summary>
        /// The response to a <see cref="Sender.RequestUserModes"/> command for this user.
        /// </summary>
        public event UserModeRequestEventHandler OnUserModeRequest;
        /// <summary>
        /// The response to a <see cref="Sender.RequestChannelModes"/> command.
        /// </summary>
        public event ChannelModeRequestEventHandler OnChannelModeRequest;
        /// <summary>
        /// A channel's mode has changed.
        /// </summary>
        public event ChannelModeChangeEventHandler OnChannelModeChange;
        /// <summary>
        /// Response to a <see cref="Sender.RequestChannelList"/> command.
        /// </summary>
        public event ChannelListEventHandler OnChannelList;
        /// <summary>
        /// The response to a <see cref="Sender.Version"/> request.
        /// </summary>
        public event VersionEventHandler OnVersion;
        /// <summary>
        /// A server's 'Message of the Day'
        /// </summary>
        public event MotdEventHandler OnMotd;
        /// <summary>
        /// The response to a <see cref="Sender.Time"/> request.
        /// </summary>
        public event TimeEventHandler OnTime;
        /// <summary>
        /// The response to an <see cref="Sender.Info"/> request.
        /// </summary>
        public event InfoEventHandler OnInfo;
        /// <summary>
        /// The response to an <see cref="Sender.Admin"/> request.
        /// </summary>
        public event AdminEventHandler OnAdmin;
        /// <summary>
        /// The response to a <see cref="Sender.Lusers"/> request.
        /// </summary>
        public event LusersEventHandler OnLusers;
        /// <summary>
        /// The response to a <see cref="Sender.Links"/> request.
        /// </summary>
        public event LinksEventHandler OnLinks;
        /// <summary>
        /// The response to a <see cref="Sender.Stats"/> request.
        /// </summary>
        public event StatsEventHandler OnStats;
        /// <summary>
        /// A User has been disconnected via a Kill message.
        /// </summary>
        public event KillEventHandler OnKill;

        private const string PING = "PING";
        private const string PONG = "PONG";
        private const string ERROR = "ERROR";
        private const string NOTICE = "NOTICE";
        private const string JOIN = "JOIN";
        private const string PRIVMSG = "PRIVMSG";
        private const string NICK = "NICK";
        private const string TOPIC = "TOPIC";
        private const string PART = "PART";
        private const string QUIT = "QUIT";
        private const string INVITE = "INVITE";
        private const string KICK = "KICK";
        private const string MODE = "MODE";
        private const string KILL = "KILL";
        private const string ACTION = "\u0001ACTION";
        private readonly char[] Separator = { ' ' };
        private readonly Regex userPattern;
        private readonly Regex channelPattern;
        private readonly Regex _replyRegex;
        /// <summary>
        /// Table to hold WhoIsInfos while they are being created. The key is the
        /// nick and the value if the WhoisInfo struct.
        /// </summary>
        private Hashtable whoisInfos;

        /// <summary>
        /// Create an instance ready to parse
        /// incoming messages.
        /// </summary>
        public Listener()
        {
            userPattern = new Regex("([\\w\\-" + Rfc2812Util.specialReg + "]+![\\~\\w]+@[\\w\\.\\-]+)", RegexOptions.Compiled | RegexOptions.Singleline);
            channelPattern = new Regex("([#!+&]\\w+)", RegexOptions.Compiled | RegexOptions.Singleline);
            _replyRegex = new Regex("^:([^\\s]*) ([\\d]{3}) ([^\\s]*) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Parse(string message)
        {
            if (OnAnything != null)
            {
                OnAnything(this, new EventArgs());
            }
            Debug.WriteLine(string.Format("RAW: \"{0}\"", message));

            string[] tokens = message.Split(Separator);

            switch (tokens[0])
            {
                case PING:
                    if (OnPing != null)
                    {
                        tokens[1] = RemoveLeadingColon(tokens[1]);
                        OnPing(CondenseStrings(tokens, 1));
                    }
                    break;
                case NOTICE:
                    if (OnPrivateNotice != null)
                    {
                        OnPrivateNotice(this, new UserMessageEventArgs(User.Empty, CondenseStrings(tokens, 2)));
                    }
                    break;
                case ERROR:
                    tokens[1] = RemoveLeadingColon(tokens[1]);
                    Error(ReplyCode.IrcServerError, CondenseStrings(tokens, 1));
                    break;
                default:
                    if (_replyRegex.IsMatch(message))
                    {
                        ParseReply(tokens);
                    }
                    else
                    {
                        ParseCommand(tokens);
                    }
                    break;
            }
        }

        /// <summary>
        /// Tell listeners that an error has been encountered
        /// </summary>
        internal void Error(ReplyCode code, string message)
        {
            if (OnError != null)
            {
                OnError(this, new ErrorMessageEventArgs(code, message));
            }
        }

        /// <summary>
        /// Parse the message and call the callback methods
        /// on the listeners.
        /// 
        /// </summary>
        /// <param name="tokens">The text received from the IRC server</param>
        private void ParseCommand(string[] tokens)
        {
            //Remove colon user info string
            tokens[0] = RemoveLeadingColon(tokens[0]);
            switch (tokens[1])
            {
                case PONG:
                    break;
                case NOTICE:
                    ProcessNoticeCommand(tokens);
                    break;
                case JOIN:
                    ProcessJoinCommand(tokens);
                    break;
                case PRIVMSG:
                    ProcessPrivmsgCommand(tokens);
                    break;
                case NICK:
                    ProcessNickCommand(tokens);
                    break;
                case TOPIC:
                    ProcessTopicCommand(tokens);
                    break;
                case PART:
                    ProcessPartCommand(tokens);
                    break;
                case QUIT:
                    ProcessQuitCommand(tokens);
                    break;
                case INVITE:
                    ProcessInviteCommand(tokens);
                    break;
                case KICK:
                    ProcessKickCommand(tokens);
                    break;
                case MODE:
                    ProcessModeCommand(tokens);
                    break;
                case KILL:
                    ProcessKillCommand(tokens);
                    break;
                default:
                    if (OnError != null)
                    {
                        OnError(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)));
                    }
                    Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseCommand() Unknown IRC command=" + tokens[1]);
                    //Trace.WriteLine("Unknown command", "IRC");
                    break;
            }
        }

        private void ProcessKillCommand(string[] tokens)
        {
            if (OnKill == null) return;

            string reason = "";
            if (tokens.Length >= 4)
            {
                tokens[3] = RemoveLeadingColon(tokens[3]);
                reason = CondenseStrings(tokens, 3);
            }
            OnKill(Rfc2812Util.UserFromString(tokens[0]), tokens[2], reason);
        }

        private void ProcessModeCommand(string[] tokens)
        {
            if (channelPattern.IsMatch(tokens[2]))
            {
                if (OnChannelModeChange == null) return;

                User who = Rfc2812Util.UserFromString(tokens[0]);
                try
                {
                    ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(tokens, 3);
                    string raw = CondenseStrings(tokens, 3);
                    OnChannelModeChange(who, tokens[2], modes, raw);
                    Trace.WriteLine("Channel mode change", "IRC");
                }
                catch (Exception)
                {
                    if (OnError != null)
                    {
                        OnError(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)));
                    }
                    Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                        "[" + Thread.CurrentThread.Name + "] Listener::ParseCommand() Bad IRC MODE string=" + tokens[0]);
                }
            }
            else
            {
                if (OnUserModeChange == null) return;

                tokens[3] = RemoveLeadingColon(tokens[3]);
                OnUserModeChange(this, new UserModeChangeEventArgs(Rfc2812Util.CharToModeAction(tokens[3][0]),
                    Rfc2812Util.CharToUserMode(tokens[3][1])));
                //Trace.WriteLine("User mode change", "IRC");
            }
        }

        private void ProcessKickCommand(string[] tokens)
        {
            if (OnKick == null) return;

            tokens[4] = RemoveLeadingColon(tokens[4]);
            OnKick(Rfc2812Util.UserFromString(tokens[0]), tokens[2], tokens[3], CondenseStrings(tokens, 4));
            //Trace.WriteLine("Kick", "IRC");
        }

        private void ProcessInviteCommand(string[] tokens)
        {
            if (OnInvite == null) return;

            OnInvite(Rfc2812Util.UserFromString(tokens[0]), RemoveLeadingColon(tokens[3]));
            //Trace.WriteLine("Invite", "IRC");
        }

        private void ProcessQuitCommand(string[] tokens)
        {
            if (OnQuit == null) return;

            tokens[2] = RemoveLeadingColon(tokens[2]);
            OnQuit(Rfc2812Util.UserFromString(tokens[0]), CondenseStrings(tokens, 2));
            //Trace.WriteLine("Quit", "IRC");
        }

        private void ProcessPartCommand(string[] tokens)
        {
            if (OnPart == null) return;

            OnPart(
                Rfc2812Util.UserFromString(tokens[0]),
                RemoveLeadingColon(tokens[2]),
                tokens.Length >= 4 ? RemoveLeadingColon(CondenseStrings(tokens, 3)) : "");
            //Trace.WriteLine("Part", "IRC");
        }

        private void ProcessTopicCommand(string[] tokens)
        {
            if (OnTopicChanged == null) return;

            tokens[3] = RemoveLeadingColon(tokens[3]);
            OnTopicChanged(this, new UserChannelMessageEventArgs(
                Rfc2812Util.UserFromString(tokens[0]), tokens[2], CondenseStrings(tokens, 3)));
            //Trace.WriteLine("Topic changed", "IRC");
        }

        private void ProcessNickCommand(string[] tokens)
        {
            if (OnNick == null) return;

            OnNick(this, new NickChangeEventArgs(Rfc2812Util.UserFromString(tokens[0]), RemoveLeadingColon(tokens[2])));
            //Trace.WriteLine("Nick", "IRC");
        }

        private void ProcessJoinCommand(string[] tokens)
        {
            if (OnJoin == null) return;

            OnJoin(Rfc2812Util.UserFromString(tokens[0]), RemoveLeadingColon(tokens[2]));
            //Trace.WriteLine("Join", "IRC");
        }

        private void ProcessNoticeCommand(string[] tokens)
        {
            tokens[3] = RemoveLeadingColon(tokens[3]);
            if (Rfc2812Util.IsValidChannelName(tokens[2]))
            {
                if (OnPublicNotice == null) return;

                OnPublicNotice(this, new UserChannelMessageEventArgs(
                    Rfc2812Util.UserFromString(tokens[0]),
                    tokens[2],
                    CondenseStrings(tokens, 3)));
                //Trace.WriteLine("Public notice", "IRC");
            }
            else
            {
                if (OnPrivateNotice == null) return;

                OnPrivateNotice(this, new UserMessageEventArgs(
                    Rfc2812Util.UserFromString(tokens[0]),
                    CondenseStrings(tokens, 3)));
                //Trace.WriteLine("Private notice", "IRC");
            }
        }

        public void ProcessPrivmsgCommand(string[] tokens)
        {
            tokens[3] = RemoveLeadingColon(tokens[3]);
            if (tokens[3] == ACTION)
            {
                if (Rfc2812Util.IsValidChannelName(tokens[2]))
                {
                    if (OnAction == null) return;

                    int last = tokens.Length - 1;
                    tokens[last] = RemoveTrailingChar(tokens[last]);
                    OnAction(this,
                        new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), tokens[2],
                            CondenseStrings(tokens, 4)));
                    //Trace.WriteLine("Channel action", "IRC");
                }
                else
                {
                    if (OnPrivateAction == null) return;

                    int last = tokens.Length - 1;
                    tokens[last] = RemoveTrailingChar(tokens[last]);
                    OnPrivateAction(this,
                        new UserMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), CondenseStrings(tokens, 4)));
                    //Trace.WriteLine("Private action", "IRC");
                }
            }
            else if (channelPattern.IsMatch(tokens[2]))
            {
                if (OnPublic == null) return;

                OnPublic(this,
                    new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), tokens[2],
                        CondenseStrings(tokens, 3)));
                Trace.WriteLine("Public msg", "IRC");
            }
            else
            {
                if (OnPrivate == null) return;

                OnPrivate(this, new UserMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), CondenseStrings(tokens, 3)));
                //Trace.WriteLine("Private msg", "IRC");
            }
        }

        public void ProcessPrivmsgCommand(IrcMessage message)
        {
            var msgTokens = message.Message.Split(Separator);
            if (RemoveLeadingColon(msgTokens[0]) == ACTION)
            {
                if (Rfc2812Util.IsValidChannelName(message.Target))
                {
                    OnAction.Fire(this, new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Target,
                            CleanActionMessage(message.Message)));
                    //Trace.WriteLine("Channel action", "IRC");
                }
                else
                {
                    OnPrivateAction.Fire(this,
                        new UserMessageEventArgs(Rfc2812Util.UserFromString(message.From), CleanActionMessage(message.Message)));
                    //Trace.WriteLine("Private action", "IRC");
                }
            }
            else if (channelPattern.IsMatch(message.Target))
            {
                OnPublic.Fire(this,
                    new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Target,
                        message.Message));
                Trace.WriteLine("Public msg", "IRC");
            }
            else
            {
                OnPrivate.Fire(this, new UserMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Message));
                //Trace.WriteLine("Private msg", "IRC");
            }
        }

        private void ParseReply(string[] tokens)
        {
            ReplyCode code = (ReplyCode)int.Parse(tokens[1], CultureInfo.InvariantCulture);
            tokens[3] = RemoveLeadingColon(tokens[3]);
            switch (code)
            {
                //Messages sent upon successful registration 
                case ReplyCode.RPL_WELCOME:
                case ReplyCode.RPL_YOURESERVICE:
                    if (OnRegistered != null)
                    {
                        OnRegistered(this, new EventArgs());
                    }
                    break;
                case ReplyCode.RPL_MOTDSTART:
                case ReplyCode.RPL_MOTD:
                    if (OnMotd != null)
                    {
                        OnMotd(CondenseStrings(tokens, 3), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFMOTD:
                    if (OnMotd != null)
                    {
                        OnMotd(CondenseStrings(tokens, 3), true);
                    }
                    break;
                case ReplyCode.RPL_ISON:
                    if (OnIson != null)
                    {
                        OnIson(tokens[3]);
                    }
                    break;
                case ReplyCode.RPL_NAMREPLY:
                    ProcessNamesReply(tokens, false);
                    break;
                case ReplyCode.RPL_ENDOFNAMES:
                    ProcessNamesReply(tokens, true);
                    break;
                case ReplyCode.RPL_LIST:
                    if (OnList != null)
                    {
                        tokens[5] = RemoveLeadingColon(tokens[5]);
                        OnList(
                            tokens[3],
                            int.Parse(tokens[4], CultureInfo.InvariantCulture),
                            CondenseStrings(tokens, 5),
                            false);
                    }
                    break;
                case ReplyCode.RPL_LISTEND:
                    if (OnList != null)
                    {
                        OnList("", 0, "", true);
                    }
                    break;
                case ReplyCode.ERR_NICKNAMEINUSE:
                case ReplyCode.ERR_NICKCOLLISION:
                    if (OnNickError != null)
                    {
                        tokens[4] = RemoveLeadingColon(tokens[4]);
                        OnNickError(this, new NickErrorEventArgs(tokens[3], CondenseStrings(tokens, 4)));
                        //Trace.WriteLine("Nick collision", "IRC");
                    }
                    break;
                case ReplyCode.RPL_NOTOPIC:
                    if (OnError != null)
                    {
                        OnError(this, new ErrorMessageEventArgs(code, CondenseStrings(tokens, 3)));
                    }
                    break;
                case ReplyCode.RPL_TOPIC:
                    if (OnRecieveTopic != null)
                    {
                        tokens[4] = RemoveLeadingColon(tokens[4]);
                        OnRecieveTopic(tokens[3], CondenseStrings(tokens, 4));
                    }
                    break;
                case ReplyCode.RPL_INVITING:
                    if (OnInviteSent != null)
                    {
                        OnInviteSent(this, new InviteSentEventArgs(tokens[3], tokens[4]));
                    }
                    break;
                case ReplyCode.RPL_AWAY:
                    if (OnAway != null)
                    {
                        OnAway(this, new AwayEventArgs(tokens[3], RemoveLeadingColon(CondenseStrings(tokens, 4))));
                    }
                    break;
                case ReplyCode.RPL_WHOREPLY:
                    if (OnWho != null)
                    {
                        User user = new User(tokens[7], tokens[4], tokens[5]);
                        OnWho(
                            user,
                            tokens[3],
                            tokens[6],
                            tokens[8],
                            int.Parse(RemoveLeadingColon(tokens[9]), CultureInfo.InvariantCulture),
                            tokens[10],
                            false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFWHO:
                    if (OnWho != null)
                    {
                        OnWho(User.Empty, "", "", "", 0, "", true);
                    }
                    break;
                case ReplyCode.RPL_WHOISUSER:
                    User whoUser = new User(tokens[3], tokens[4], tokens[5]);
                    WhoisInfo whoisInfo = LookupInfo(whoUser.Nick);
                    whoisInfo.user = whoUser;
                    tokens[7] = RemoveLeadingColon(tokens[7]);
                    whoisInfo.realName = CondenseStrings(tokens, 7);
                    break;
                case ReplyCode.RPL_WHOISCHANNELS:
                    WhoisInfo whoisChannelInfo = LookupInfo(tokens[3]);
                    tokens[4] = RemoveLeadingColon(tokens[4]);
                    int numberOfChannels = tokens.Length - 4;
                    string[] channels = new String[numberOfChannels];
                    Array.Copy(tokens, 4, channels, 0, numberOfChannels);
                    whoisChannelInfo.SetChannels(channels);
                    break;
                case ReplyCode.RPL_WHOISSERVER:
                    WhoisInfo whoisServerInfo = LookupInfo(tokens[3]);
                    whoisServerInfo.ircServer = tokens[4];
                    tokens[5] = RemoveLeadingColon(tokens[5]);
                    whoisServerInfo.serverDescription = CondenseStrings(tokens, 5);
                    break;
                case ReplyCode.RPL_WHOISOPERATOR:
                    WhoisInfo whoisOpInfo = LookupInfo(tokens[3]);
                    whoisOpInfo.isOperator = true;
                    break;
                case ReplyCode.RPL_WHOISIDLE:
                    WhoisInfo whoisIdleInfo = LookupInfo(tokens[3]);
                    whoisIdleInfo.idleTime = long.Parse(tokens[5], CultureInfo.InvariantCulture);
                    break;
                case ReplyCode.RPL_ENDOFWHOIS:
                    string nick = tokens[3];
                    WhoisInfo whoisEndInfo = LookupInfo(nick);
                    if (OnWhois != null)
                    {
                        OnWhois(whoisEndInfo);
                    }
                    whoisInfos.Remove(nick);
                    break;
                case ReplyCode.RPL_WHOWASUSER:
                    if (OnWhowas != null)
                    {
                        User whoWasUser = new User(tokens[3], tokens[4], tokens[5]);
                        tokens[7] = RemoveLeadingColon(tokens[7]);
                        OnWhowas(whoWasUser, CondenseStrings(tokens, 7), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFWHOWAS:
                    if (OnWhowas != null)
                    {
                        OnWhowas(User.Empty, "", true);
                    }
                    break;
                case ReplyCode.RPL_UMODEIS:
                    if (OnUserModeRequest != null)
                    {
                        //First drop the '+'
                        string chars = tokens[3].Substring(1);
                        UserMode[] modes = Rfc2812Util.UserModesToArray(chars);
                        OnUserModeRequest(modes);
                    }
                    break;
                case ReplyCode.RPL_CHANNELMODEIS:
                    if (OnChannelModeRequest != null)
                    {
                        try
                        {
                            ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(tokens, 4);
                            OnChannelModeRequest(tokens[3], modes);
                        }
                        catch (Exception)
                        {
                            if (OnError != null)
                            {
                                OnError(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)));
                            }
                            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseReply() Bad IRC MODE string=" + tokens[0]);
                        }
                    }
                    break;
                case ReplyCode.RPL_BANLIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Ban, tokens[4], Rfc2812Util.UserFromString(tokens[5]), Convert.ToInt64(tokens[6], CultureInfo.InvariantCulture), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFBANLIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Ban, "", User.Empty, 0, true);
                    }
                    break;
                case ReplyCode.RPL_INVITELIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Invitation, tokens[4], Rfc2812Util.UserFromString(tokens[5]), Convert.ToInt64(tokens[6]), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFINVITELIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Invitation, "", User.Empty, 0, true);
                    }
                    break;
                case ReplyCode.RPL_EXCEPTLIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Exception, tokens[4], Rfc2812Util.UserFromString(tokens[5]), Convert.ToInt64(tokens[6]), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFEXCEPTLIST:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.Exception, "", User.Empty, 0, true);
                    }
                    break;
                case ReplyCode.RPL_UNIQOPIS:
                    if (OnChannelList != null)
                    {
                        OnChannelList(tokens[3], ChannelMode.ChannelCreator, tokens[4], User.Empty, 0, true);
                    }
                    break;
                case ReplyCode.RPL_VERSION:
                    if (OnVersion != null)
                    {
                        OnVersion(CondenseStrings(tokens, 3));
                    }
                    break;
                case ReplyCode.RPL_TIME:
                    if (OnTime != null)
                    {
                        OnTime(CondenseStrings(tokens, 3));
                    }
                    break;
                case ReplyCode.RPL_INFO:
                    if (OnInfo != null)
                    {
                        OnInfo(CondenseStrings(tokens, 3), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFINFO:
                    if (OnInfo != null)
                    {
                        OnInfo(CondenseStrings(tokens, 3), true);
                    }
                    break;
                case ReplyCode.RPL_ADMINME:
                case ReplyCode.RPL_ADMINLOC1:
                case ReplyCode.RPL_ADMINLOC2:
                case ReplyCode.RPL_ADMINEMAIL:
                    if (OnAdmin != null)
                    {
                        OnAdmin(RemoveLeadingColon(CondenseStrings(tokens, 3)));
                    }
                    break;
                case ReplyCode.RPL_LUSERCLIENT:
                case ReplyCode.RPL_LUSEROP:
                case ReplyCode.RPL_LUSERUNKNOWN:
                case ReplyCode.RPL_LUSERCHANNELS:
                case ReplyCode.RPL_LUSERME:
                    if (OnLusers != null)
                    {
                        OnLusers(RemoveLeadingColon(CondenseStrings(tokens, 3)));
                    }
                    break;
                case ReplyCode.RPL_LINKS:
                    if (OnLinks != null)
                    {
                        OnLinks(tokens[3], //mask
                                    tokens[4], //hostname
                                    int.Parse(RemoveLeadingColon(tokens[5]), CultureInfo.InvariantCulture), //hopcount
                                    CondenseStrings(tokens, 6), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFLINKS:
                    if (OnLinks != null)
                    {
                        OnLinks(String.Empty, String.Empty, -1, String.Empty, true);
                    }
                    break;
                case ReplyCode.RPL_STATSLINKINFO:
                case ReplyCode.RPL_STATSCOMMANDS:
                case ReplyCode.RPL_STATSUPTIME:
                case ReplyCode.RPL_STATSOLINE:
                    if (OnStats != null)
                    {
                        OnStats(GetQueryType(code), RemoveLeadingColon(CondenseStrings(tokens, 3)), false);
                    }
                    break;
                case ReplyCode.RPL_ENDOFSTATS:
                    if (OnStats != null)
                    {
                        OnStats(Rfc2812Util.CharToStatsQuery(tokens[3][0]), RemoveLeadingColon(CondenseStrings(tokens, 4)), true);
                    }
                    break;
                default:
                    HandleDefaultReply(code, tokens);
                    break;
            }
        }

        public void ProcessNamesReply(string[] tokens, bool end)
        {
            if (OnNames == null) return;

            if (end)
            {
                OnNames(this, new NamesEventArgs(tokens[3], new string[0], true));
            }
            else
            {
                if (tokens[2].EndsWith("=")) //hack: Gamesurge sometimes does this
                {
                    var newtokens = new List<string>(tokens);
                    newtokens.RemoveAt(2);
                    newtokens.Insert(2, tokens[2].Remove(tokens[2].Length - 1));
                    newtokens.Insert(3, "=");
                    tokens = newtokens.ToArray();
                }
                tokens[5] = RemoveLeadingColon(tokens[5]);
                int numberOfUsers = tokens.Length - 5;
                string[] users = new string[numberOfUsers];
                Array.Copy(tokens, 5, users, 0, numberOfUsers);
                OnNames(this, new NamesEventArgs(tokens[4], users, false));
                //Trace.WriteLine("Names", "IRC");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="tokens"></param>
        private void HandleDefaultReply(ReplyCode code, string[] tokens)
        {
            if (code >= ReplyCode.ERR_NOSUCHNICK && code <= ReplyCode.ERR_USERSDONTMATCH)
            {
                if (OnError != null)
                {
                    OnError(this, new ErrorMessageEventArgs(code, CondenseStrings(tokens, 3)));
                }
            }
            else if (OnReply != null)
            {
                OnReply(this, new ReplyEventArgs(code, CondenseStrings(tokens, 3)));
            }
        }
        /// <summary>
        /// Find the correct WhoIs object based on the nick name.
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        private WhoisInfo LookupInfo(string nick)
        {
            if (whoisInfos == null)
            {
                whoisInfos = new Hashtable();
            }
            WhoisInfo info = (WhoisInfo)whoisInfos[nick];
            if (info == null)
            {
                info = new WhoisInfo();
                whoisInfos[nick] = info;
            }
            return info;
        }
        /// <summary>
        /// Turn an array of strings back into a single string.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private string CondenseStrings(string[] strings, int start)
        {
            if (strings.Length == start + 1)
            {
                return strings[start];
            }
            else
            {
                return String.Join(" ", strings, start, (strings.Length - start));
            }
        }
        public string RemoveLeadingColon(string text)
        {
            if (text[0] == ':')
            {
                return text.Substring(1);
            }
            return text;
        }
        /// <summary>
        /// Strip off the trailing char, usually a CTCP quote.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string RemoveTrailingChar(string text)
        {
            return text.Substring(0, text.Length - 1);
        }

        private StatsQuery GetQueryType(ReplyCode code)
        {
            switch (code)
            {
                case ReplyCode.RPL_STATSLINKINFO:
                    return StatsQuery.Connections;
                case ReplyCode.RPL_STATSCOMMANDS:
                    return StatsQuery.CommandUsage;
                case ReplyCode.RPL_STATSUPTIME:
                    return StatsQuery.Uptime;
                case ReplyCode.RPL_STATSOLINE:
                    return StatsQuery.Operators;
                //Should never get here
                default:
                    return StatsQuery.CommandUsage;
            }
        }

        public IrcMessage ParseIrcMessage(string message)
        {
            var msg = new IrcMessage();

            string[] tokens = message.Split(Separator);
            msg.Tokens = tokens;

            msg.From = RemoveLeadingColon(tokens[0]);
            msg.Command = tokens[1];
            msg.Target = tokens[2];

            var colonPos = message.IndexOf(" :");
            if (colonPos != -1)
            {
                msg.Message = message.Substring(colonPos + 2);
            }

            if (_replyRegex.IsMatch(message))
            {
                try
                {
                    msg.ReplyCode = (ReplyCode)int.Parse(tokens[1]);
                }
                catch (FormatException)
                {
                    msg.ReplyCode = ReplyCode.Null;
                }
            }

            return msg;
        }

        public void ProcessNamesReply(IrcMessage ircMessage)
        {
            if (OnNames == null) return;

            var tokens = ircMessage.Tokens;

            if (tokens[2].EndsWith("=")) //hack: Gamesurge sometimes does this
            {
                var newtokens = new List<string>(tokens);
                newtokens.RemoveAt(2);
                newtokens.Insert(2, tokens[2].Remove(tokens[2].Length - 1));
                newtokens.Insert(3, "=");
                tokens = newtokens.ToArray();
            }
            tokens[5] = RemoveLeadingColon(tokens[5]);
            int numberOfUsers = tokens.Length - 5;
            string[] users = new string[numberOfUsers];
            Array.Copy(tokens, 5, users, 0, numberOfUsers);
            OnNames(this, new NamesEventArgs(tokens[4], users, false));
            //Trace.WriteLine("Names", "IRC");
        }

        /// <summary>
        /// Cleans an ACTION PRIVMSG so that only the text to be displayed remains
        /// </summary>
        /// <param name="message">The raw message</param>
        /// <returns>The cleaned message</returns>
        public string CleanActionMessage(string message)
        {
            string cleaned = RemoveTrailingChar(message);
            return cleaned.Substring(8);
        }
    }
}
