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
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// This class parses messages received from the IRC server and
/// raises the appropriate event.
/// </summary>
public class Listener
{
    /// <summary>
    /// The server sent a numeric reply that is not an error and that no other event is for.
    /// </summary>
    public event EventHandler<ReplyEventArgs> OnReply;
    /// <summary>
    /// The IRC server sent an error, or FlamingIRC found an error.
    /// </summary>
    public event EventHandler<ErrorMessageEventArgs> OnError;
    /// <summary>
    /// This client sent a <see cref="Sender.PrivateNotice"/> or a
    /// <see cref="Sender.PrivateMessage"/> to a user who is away.
    /// </summary>
    public event EventHandler<AwayEventArgs> OnAway;
    /// <summary>
    /// The server confirmed that this client's user is away. This is the reply to
    /// <see cref="Sender.Away"/>.
    /// </summary>
    public event EventHandler OnNowAway;
    /// <summary>
    /// The server confirmed that this client's user is not away. This is the reply to
    /// <see cref="Sender.UnAway"/>.
    /// </summary>
    public event EventHandler OnUnAway;
    /// <summary>
    /// The server sent the invite from <see cref="Sender.Invite"/> to the other user.
    /// </summary>
    public event EventHandler<InviteEventArgs> OnInviteSent;
    /// <summary>
    /// The server did not accept a nick that this client's user tried to use.
    /// </summary>
    public event EventHandler<NickErrorEventArgs> OnNickError;
    /// <summary>
    /// The server sent a keep-alive PING. <see cref="Connection"/> replies with a PONG.
    /// </summary>
    public event EventHandler<PingEventArgs> OnPing;
    /// <summary>
    /// The Listener received a line from the server. Use this event to track activity.
    /// </summary>
    public event EventHandler OnAnything;
    /// <summary>
    /// The connection to the IRC server is open and registered.
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
    /// A user sent a NOTICE to a channel.
    /// </summary>
    public event EventHandler<UserChannelMessageEventArgs> OnPublicNotice;
    /// <summary>
    /// A user or the server sent a NOTICE to this client.
    /// </summary>
    public event EventHandler<UserMessageEventArgs> OnPrivateNotice;
    /// <summary>
    /// A user joined a channel.
    /// </summary>
    public event EventHandler<JoinEventArgs> OnJoin;
    /// <summary>
    /// A user sent a message to a channel.
    /// </summary>
    public event EventHandler<UserChannelMessageEventArgs> OnPublic;
    /// <summary>
    /// A user sent an action (/me) to a channel.
    /// </summary>
    public event EventHandler<UserChannelMessageEventArgs> OnAction;
    /// <summary>
    /// A user sent an action (/me) to this client.
    /// </summary>
    public event EventHandler<UserMessageEventArgs> OnPrivateAction;
    /// <summary>
    /// A user changed nick.
    /// </summary>
    public event EventHandler<NickChangeEventArgs> OnNick;
    /// <summary>
    /// A user sent a private message to this client.
    /// </summary>
    public event EventHandler<UserMessageEventArgs> OnPrivate;
    /// <summary>
    /// A user changed the topic of a channel.
    /// </summary>
    public event EventHandler<UserChannelMessageEventArgs> OnTopicChanged;
    /// <summary>
    /// The response to a <see cref="Sender.RequestTopic"/> command.
    /// </summary>
    public event EventHandler<TopicEventArgs> OnReceiveTopic;
    /// <summary>
    /// A user left a channel.
    /// </summary>
    public event EventHandler<PartEventArgs> OnPart;
    /// <summary>
    /// A user quit IRC.
    /// </summary>
    public event EventHandler<QuitEventArgs> OnQuit;
    /// <summary>
    /// A user invited this client's user to a channel.
    /// </summary>
    public event EventHandler<InviteEventArgs> OnInvite;
    /// <summary>
    /// A user removed (kicked) a user from a channel.
    /// </summary>
    public event EventHandler<KickEventArgs> OnKick;
    /// <summary>
    /// The response to a <see cref="Sender.Names"/> request.
    /// </summary>
    public event EventHandler<NamesEventArgs> OnNames;
    /// <summary>
    /// The response to a <see cref="Sender.List"/> request.
    /// </summary>
    public event EventHandler<ListEventArgs> OnList;
    /// <summary>
    /// The response to a <see cref="Sender.Ison"/> request.
    /// </summary>
    public event EventHandler<IsonEventArgs> OnIson;
    /// <summary>
    /// The response to a <see cref="Sender.Who"/> request.
    /// </summary>
    public event EventHandler<WhoEventArgs> OnWho;
    /// <summary>
    /// The response to a <see cref="Sender.Whois"/> request.
    /// </summary>
    public event EventHandler<WhoisEventArgs> OnWhois;
    /// <summary>
    /// The response to a <see cref="Sender.Whowas"/> request.
    /// </summary>
    public event EventHandler<WhowasEventArgs> OnWhowas;
    /// <summary>
    /// A mode of this client's user changed.
    /// </summary>
    public event EventHandler<UserModeChangeEventArgs> OnUserModeChange;
    /// <summary>
    /// The response to a <see cref="Sender.RequestUserModes"/> command for this user.
    /// </summary>
    public event EventHandler<UserModeRequestEventArgs> OnUserModeRequest;
    /// <summary>
    /// The response to a <see cref="Sender.RequestChannelModes"/> command.
    /// </summary>
    public event EventHandler<ChannelModeRequestEventArgs> OnChannelModeRequest;
    /// <summary>
    /// A user or the server changed one or more modes of a channel.
    /// </summary>
    public event EventHandler<ChannelModeChangeEventArgs> OnChannelModeChange;
    /// <summary>
    /// The response to a <see cref="Sender.RequestChannelList"/> command.
    /// </summary>
    public event EventHandler<ChannelListEventArgs> OnChannelList;
    /// <summary>
    /// The response to a <see cref="Sender.Version"/> request.
    /// </summary>
    public event EventHandler<VersionEventArgs> OnVersion;
    /// <summary>
    /// One line of the server's Message of the Day.
    /// </summary>
    public event EventHandler<MotdEventArgs> OnMotd;
    /// <summary>
    /// The response to a <see cref="Sender.Time"/> request.
    /// </summary>
    public event EventHandler<TimeEventArgs> OnTime;
    /// <summary>
    /// The response to an <see cref="Sender.Info"/> request.
    /// </summary>
    public event EventHandler<InfoEventArgs> OnInfo;
    /// <summary>
    /// The response to an <see cref="Sender.Admin"/> request.
    /// </summary>
    public event EventHandler<AdminEventArgs> OnAdmin;
    /// <summary>
    /// The response to a <see cref="Sender.Lusers"/> request.
    /// </summary>
    public event EventHandler<LusersEventArgs> OnLusers;
    /// <summary>
    /// The response to a <see cref="Sender.Links"/> request.
    /// </summary>
    public event EventHandler<LinksEventArgs> OnLinks;
    /// <summary>
    /// The response to a <see cref="Sender.Stats"/> request.
    /// </summary>
    public event EventHandler<StatsEventArgs> OnStats;
    /// <summary>
    /// An operator or a server disconnected a user with a KILL message.
    /// </summary>
    public event EventHandler<KillEventArgs> OnKill;

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
    private readonly Regex channelPattern;
    private readonly Regex _replyRegex;
    private readonly Regex _propertiesRegex;

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
        channelPattern = new Regex("([#!+&]\\w+)", RegexOptions.Compiled | RegexOptions.Singleline);
        _replyRegex = new Regex("^:([^\\s]*) ([\\d]{3}) ([^\\s]*) (.*)", RegexOptions.Compiled | RegexOptions.Singleline);
        _propertiesRegex = new Regex("([A-Z]+)=([^\\s]+)", RegexOptions.Compiled | RegexOptions.Singleline);
    }

    /// <summary>
    /// The '005' (ISUPPORT) properties of the server that sends the messages. The Listener
    /// records each '005' reply here, and its mode parser reads CHANMODES and PREFIX from them.
    /// </summary>
    internal ServerProperties ServerProperties { get; private set; } = new ServerProperties();

    /// <summary>
    /// Forget the properties of the last server. Call this before a new session starts.
    /// </summary>
    internal void ResetServerProperties() => ServerProperties = new ServerProperties();

    /// <summary>
    /// Parses and handles raw server messages
    /// </summary>
    /// <param name="message">The raw message from the server</param>
    public void Parse(string message)
    {
        OnAnything?.Invoke(this, new EventArgs());
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::Parse() RAW: \"{message}\"");

        IrcMessage ircMessage = ParseIrcMessage(message);

        switch (ircMessage.Tokens[0])
        {
            case PING:
                OnPing?.Invoke(this, new PingEventArgs(ircMessage.Message));
                break;
            case NOTICE:
                OnPrivateNotice?.Invoke(this, new UserMessageEventArgs(User.Empty, ircMessage.Message));
                break;
            case ERROR:
                Error(ReplyCode.IrcServerError, ircMessage.Message);
                break;
            default:
                if (_replyRegex.IsMatch(message))
                {
                    ParseReply(ircMessage.Tokens);
                }
                else
                {
                    ParseCommand(ircMessage);
                }
                break;
        }
    }

    /// <summary>
    /// Dispatches an incoming server command to the appropriate handler
    /// based on <see cref="IrcMessage.Command"/>.
    /// </summary>
    /// <param name="ircMessage">The parsed message, with From, Target, and Message pre-populated by <see cref="ParseIrcMessage"/>.</param>
    private void ParseCommand(IrcMessage ircMessage)
    {
        switch (ircMessage.Command)
        {
            case PONG:
                break;
            case NOTICE:
                ProcessNoticeCommand(ircMessage);
                break;
            case JOIN:
                ProcessJoinCommand(ircMessage);
                break;
            case PRIVMSG:
                ProcessPrivmsgCommand(ircMessage);
                break;
            case NICK:
                ProcessNickCommand(ircMessage);
                break;
            case TOPIC:
                ProcessTopicCommand(ircMessage);
                break;
            case PART:
                ProcessPartCommand(ircMessage);
                break;
            case QUIT:
                ProcessQuitCommand(ircMessage);
                break;
            case INVITE:
                ProcessInviteCommand(ircMessage);
                break;
            case KICK:
                ProcessKickCommand(ircMessage);
                break;
            case MODE:
                ProcessModeCommand(ircMessage);
                break;
            case KILL:
                ProcessKillCommand(ircMessage);
                break;
            default:
                OnError?.Invoke(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, ircMessage.Message));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    $"[{Thread.CurrentThread.Name}] Listener::ParseCommand() Unknown IRC command={ircMessage.Command}");
                break;
        }
    }

    /// <summary>
    /// Tell listeners that an error has been encountered
    /// </summary>
    internal void Error(ReplyCode code, string message)
        => OnError?.Invoke(this, new ErrorMessageEventArgs(code, message));

    private void ProcessKillCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessKillCommand() target={ircMessage.Target}");
        OnKill?.Invoke(this, new KillEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), ircMessage.Target,
            ircMessage.Message ?? string.Empty));
    }

    private void ProcessModeCommand(IrcMessage ircMessage)
    {
        if (channelPattern.IsMatch(ircMessage.Target))
        {
            if (OnChannelModeChange == null) return;

            User who = Rfc2812Util.UserFromString(ircMessage.From);
            try
            {
                ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(ircMessage.Tokens, 3, ChannelModeSyntax.FromServerProperties(ServerProperties));
                string raw = CondenseStrings(ircMessage.Tokens, 3);
                OnChannelModeChange?.Invoke(this,
                    new ChannelModeChangeEventArgs(who, ircMessage.Target, modes, raw));
            }
            catch (Exception)
            {
                OnError?.Invoke(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, ircMessage.Message));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    "[" + Thread.CurrentThread.Name + "] Listener::ParseCommand() Bad IRC MODE string=" + ircMessage.From);
            }
        }
        else
        {
            string modeStr = RemoveLeadingColon(ircMessage.Tokens[3]);
            OnUserModeChange?.Invoke(this, new UserModeChangeEventArgs(Rfc2812Util.CharToModeAction(modeStr[0]),
                Rfc2812Util.CharToUserMode(modeStr[1])));
        }
    }

    public void ProcessKickCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessKickCommand() channel={ircMessage.Tokens[2]}");
        OnKick?.Invoke(this, new KickEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), ircMessage.Tokens[2], ircMessage.Tokens[3],
            ircMessage.Message));
    }

    public void ProcessInviteCommand(string[] tokens)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessInviteCommand()");
        OnInvite?.Invoke(this, new InviteEventArgs(tokens[0], RemoveLeadingColon(tokens[3])));
    }

    private void ProcessQuitCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessQuitCommand() from={ircMessage.From}");
        OnQuit?.Invoke(this, new QuitEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), ircMessage.Message ?? string.Empty));
    }

    private void ProcessPartCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessPartCommand() channel={ircMessage.Target}");
        OnPart?.Invoke(this, new PartEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), ircMessage.Target,
            ircMessage.Message ?? string.Empty));
    }

    private void ProcessTopicCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessTopicCommand() channel={ircMessage.Target}");
        OnTopicChanged?.Invoke(this, new UserChannelMessageEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), ircMessage.Target, ircMessage.Message));
    }

    public void ProcessNickCommand(IrcMessage message)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessNickCommand()");
        OnNick?.Invoke(this, new NickChangeEventArgs(Rfc2812Util.UserFromString(message.From), message.Message));
    }

    public void ProcessJoinCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessJoinCommand() channel={ircMessage.Target}");
        OnJoin?.Invoke(this, new JoinEventArgs(
            Rfc2812Util.UserFromString(ircMessage.From), RemoveLeadingColon(ircMessage.Target)));
    }

    public void ProcessNoticeCommand(string[] tokens)
    {
        tokens[3] = RemoveLeadingColon(tokens[3]);
        if (Rfc2812Util.IsValidChannelName(tokens[2]))
        {
            OnPublicNotice?.Invoke(this, new UserChannelMessageEventArgs(
                Rfc2812Util.UserFromString(tokens[0]),
                tokens[2],
                CondenseStrings(tokens, 3)));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessNoticeCommand() public channel={tokens[2]}");
        }
        else
        {
            OnPrivateNotice?.Invoke(this, new UserMessageEventArgs(
                Rfc2812Util.UserFromString(tokens[0]),
                CondenseStrings(tokens, 3)));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessNoticeCommand() private");
        }
    }

    public void ProcessPrivmsgCommand(string[] tokens)
    {
        tokens[3] = RemoveLeadingColon(tokens[3]);
        if (tokens[3] == ACTION)
        {
            if (Rfc2812Util.IsValidChannelName(tokens[2]))
            {
                int last = tokens.Length - 1;
                tokens[last] = RemoveTrailingChar(tokens[last]);
                OnAction?.Invoke(this,
                    new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), tokens[2],
                        CondenseStrings(tokens, 4)));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                    $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() channel action channel={tokens[2]}");
            }
            else
            {
                int last = tokens.Length - 1;
                tokens[last] = RemoveTrailingChar(tokens[last]);
                OnPrivateAction?.Invoke(this,
                    new UserMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), CondenseStrings(tokens, 4)));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                    $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() private action");
            }
        }
        else if (channelPattern.IsMatch(tokens[2]))
        {
            OnPublic?.Invoke(this,
                new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), tokens[2],
                    CondenseStrings(tokens, 3)));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() public channel={tokens[2]}");
        }
        else
        {
            OnPrivate?.Invoke(this, new UserMessageEventArgs(Rfc2812Util.UserFromString(tokens[0]), CondenseStrings(tokens, 3)));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() private");
        }
    }

    public void ProcessPrivmsgCommand(IrcMessage message)
    {
        var msgTokens = message.Message.Split(Separator);
        if (msgTokens[0] == ACTION)
        {
            if (Rfc2812Util.IsValidChannelName(message.Target))
            {
                OnAction?.Invoke(this, new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Target,
                        CleanActionMessage(message.Message)));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                    $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() channel action channel={message.Target}");
            }
            else
            {
                OnPrivateAction?.Invoke(this,
                    new UserMessageEventArgs(Rfc2812Util.UserFromString(message.From), CleanActionMessage(message.Message)));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                    $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() private action");
            }
        }
        else if (channelPattern.IsMatch(message.Target))
        {
            OnPublic?.Invoke(this,
                new UserChannelMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Target,
                    message.Message));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() public channel={message.Target}");
        }
        else
        {
            OnPrivate?.Invoke(this, new UserMessageEventArgs(Rfc2812Util.UserFromString(message.From), message.Message));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessPrivmsgCommand() private");
        }
    }

    public void ParseReply(string[] tokens)
    {
        ReplyCode code = (ReplyCode)int.Parse(tokens[1], CultureInfo.InvariantCulture);
        tokens[3] = RemoveLeadingColon(tokens[3]);
        switch (code)
        {
            //Messages sent upon successful registration 
            case ReplyCode.RPL_WELCOME:
            case ReplyCode.RPL_YOURESERVICE:
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceInfo,
                    $"[{Thread.CurrentThread.Name}] Listener::ParseReply() Registered");
                OnRegistered?.Invoke(this, new EventArgs());
                break;
            case ReplyCode.RPL_MOTDSTART:
            case ReplyCode.RPL_MOTD:
                OnMotd?.Invoke(this, new MotdEventArgs(CondenseStrings(tokens, 3), false));
                break;
            case ReplyCode.RPL_ENDOFMOTD:
                OnMotd?.Invoke(this, new MotdEventArgs(CondenseStrings(tokens, 3), true));
                break;
            case ReplyCode.RPL_ISON:
                OnIson?.Invoke(this, new IsonEventArgs(tokens[3]));
                break;
            case ReplyCode.RPL_NAMREPLY:
                ProcessNamesReply(tokens, false);
                break;
            case ReplyCode.RPL_ENDOFNAMES:
                ProcessNamesReply(tokens, true);
                break;
            case ReplyCode.RPL_LIST:
                tokens[5] = RemoveLeadingColon(tokens[5]);
                OnList?.Invoke(this, new ListEventArgs(
                    tokens[3],
                    int.Parse(tokens[4], CultureInfo.InvariantCulture),
                    CondenseStrings(tokens, 5),
                    false));
                break;
            case ReplyCode.RPL_LISTEND:
                OnList?.Invoke(this, new ListEventArgs(string.Empty, 0, string.Empty, true));
                break;
            case ReplyCode.ERR_NICKNAMEINUSE:
            case ReplyCode.ERR_NICKCOLLISION:
                tokens[4] = RemoveLeadingColon(tokens[4]);
                OnNickError?.Invoke(this, new NickErrorEventArgs(tokens[3], CondenseStrings(tokens, 4)));
                Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning,
                    $"[{Thread.CurrentThread.Name}] Listener::ParseReply() Nick collision nick={tokens[3]}");
                break;
            case ReplyCode.RPL_NOTOPIC:
                OnError?.Invoke(this, new ErrorMessageEventArgs(code, CondenseStrings(tokens, 3)));
                break;
            case ReplyCode.RPL_TOPIC:
                tokens[4] = RemoveLeadingColon(tokens[4]);
                OnReceiveTopic?.Invoke(this, new TopicEventArgs(
                    tokens[3], CondenseStrings(tokens, 4)));
                break;
            case ReplyCode.RPL_INVITING:
                OnInviteSent?.Invoke(this, new InviteEventArgs(tokens[3], tokens[4]));
                break;
            case ReplyCode.RPL_AWAY:
                OnAway?.Invoke(this, new AwayEventArgs(tokens[3], RemoveLeadingColon(CondenseStrings(tokens, 4))));
                break;
            case ReplyCode.RPL_NOWAWAY:
                OnNowAway?.Invoke(this, EventArgs.Empty);
                break;
            case ReplyCode.RPL_UNAWAY:
                OnUnAway?.Invoke(this, EventArgs.Empty);
                break;
            case ReplyCode.RPL_WHOREPLY:
                User user = new User(tokens[7], tokens[4], tokens[5]);
                OnWho?.Invoke(this, new WhoEventArgs(
                    user,
                    tokens[3],
                    tokens[6],
                    tokens[8],
                    int.Parse(RemoveLeadingColon(tokens[9]), CultureInfo.InvariantCulture),
                    tokens[10],
                    false));
                break;
            case ReplyCode.RPL_ENDOFWHO:
                OnWho?.Invoke(this, new WhoEventArgs(
                    User.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty, true));
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
                string[] channels = new string[numberOfChannels];
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
                OnWhois?.Invoke(this, new WhoisEventArgs(whoisEndInfo));
                whoisInfos.Remove(nick);
                break;
            case ReplyCode.RPL_WHOWASUSER:
                User whoWasUser = new User(tokens[3], tokens[4], tokens[5]);
                tokens[7] = RemoveLeadingColon(tokens[7]);
                OnWhowas?.Invoke(this, new WhowasEventArgs(
                    whoWasUser, CondenseStrings(tokens, 7), false));
                break;
            case ReplyCode.RPL_ENDOFWHOWAS:
                OnWhowas?.Invoke(this, new WhowasEventArgs(User.Empty, string.Empty, true));
                break;
            case ReplyCode.RPL_UMODEIS:
                {
                    //First drop the '+'
                    string chars = tokens[3].Substring(1);
                    UserMode[] modes = Rfc2812Util.UserModesToArray(chars);
                    OnUserModeRequest?.Invoke(this, new UserModeRequestEventArgs(modes));
                }
                break;
            case ReplyCode.RPL_BOUNCE:
                foreach (Match match in _propertiesRegex.Matches(CondenseStrings(tokens, 3)))
                    ServerProperties.SetProperty(match.Groups[1].Value, match.Groups[2].Value);
                HandleDefaultReply(code, tokens);
                break;
            case ReplyCode.RPL_CHANNELMODEIS:
                try
                {
                    ChannelModeInfo[] modes = ChannelModeInfo.ParseModes(tokens, 4, ChannelModeSyntax.FromServerProperties(ServerProperties));
                    OnChannelModeRequest?.Invoke(this, new ChannelModeRequestEventArgs(
                        tokens[3], modes));
                }
                catch (Exception)
                {
                    OnError?.Invoke(this, new ErrorMessageEventArgs(ReplyCode.UnparseableMessage, CondenseStrings(tokens, 0)));
                    Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceWarning, "[" + Thread.CurrentThread.Name + "] Listener::ParseReply() Bad IRC MODE string=" + tokens[0]);
                }
                break;
            case ReplyCode.RPL_BANLIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Ban, tokens[4], Rfc2812Util.UserFromString(tokens[5]),
                    Convert.ToInt64(tokens[6], CultureInfo.InvariantCulture), false));
                break;
            case ReplyCode.RPL_ENDOFBANLIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Ban, string.Empty, User.Empty, 0, true));
                break;
            case ReplyCode.RPL_INVITELIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Invitation, tokens[4],
                    Rfc2812Util.UserFromString(tokens[5]),
                    Convert.ToInt64(tokens[6], CultureInfo.InvariantCulture), false));
                break;
            case ReplyCode.RPL_ENDOFINVITELIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Invitation, string.Empty, User.Empty, 0, true));
                break;
            case ReplyCode.RPL_EXCEPTLIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Exception, tokens[4],
                    Rfc2812Util.UserFromString(tokens[5]),
                    Convert.ToInt64(tokens[6], CultureInfo.InvariantCulture), false));
                break;
            case ReplyCode.RPL_ENDOFEXCEPTLIST:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.Exception, string.Empty, User.Empty, 0, true));
                break;
            case ReplyCode.RPL_UNIQOPIS:
                OnChannelList?.Invoke(this, new ChannelListEventArgs(
                    tokens[3], ChannelMode.ChannelCreator, tokens[4], User.Empty, 0, true));
                break;
            case ReplyCode.RPL_VERSION:
                OnVersion?.Invoke(this, new VersionEventArgs(CondenseStrings(tokens, 3)));
                break;
            case ReplyCode.RPL_TIME:
                OnTime?.Invoke(this, new TimeEventArgs(CondenseStrings(tokens, 3)));
                break;
            case ReplyCode.RPL_INFO:
                OnInfo?.Invoke(this, new InfoEventArgs(CondenseStrings(tokens, 3), false));
                break;
            case ReplyCode.RPL_ENDOFINFO:
                OnInfo?.Invoke(this, new InfoEventArgs(CondenseStrings(tokens, 3), true));
                break;
            case ReplyCode.RPL_ADMINME:
            case ReplyCode.RPL_ADMINLOC1:
            case ReplyCode.RPL_ADMINLOC2:
            case ReplyCode.RPL_ADMINEMAIL:
                OnAdmin?.Invoke(this, new AdminEventArgs(
                    RemoveLeadingColon(CondenseStrings(tokens, 3))));
                break;
            case ReplyCode.RPL_LUSERCLIENT:
            case ReplyCode.RPL_LUSEROP:
            case ReplyCode.RPL_LUSERUNKNOWN:
            case ReplyCode.RPL_LUSERCHANNELS:
            case ReplyCode.RPL_LUSERME:
                OnLusers?.Invoke(this, new LusersEventArgs(
                    RemoveLeadingColon(CondenseStrings(tokens, 3))));
                break;
            case ReplyCode.RPL_LINKS:
                OnLinks?.Invoke(this, new LinksEventArgs(tokens[3], //mask
                    tokens[4], //hostname
                    int.Parse(RemoveLeadingColon(tokens[5]), CultureInfo.InvariantCulture), //hopcount
                    CondenseStrings(tokens, 6), false));
                break;
            case ReplyCode.RPL_ENDOFLINKS:
                OnLinks?.Invoke(this, new LinksEventArgs(
                    string.Empty, string.Empty, -1, string.Empty, true));
                break;
            case ReplyCode.RPL_STATSLINKINFO:
            case ReplyCode.RPL_STATSCOMMANDS:
            case ReplyCode.RPL_STATSUPTIME:
            case ReplyCode.RPL_STATSOLINE:
                OnStats?.Invoke(this, new StatsEventArgs(
                    GetQueryType(code), RemoveLeadingColon(CondenseStrings(tokens, 3)), false));
                break;
            case ReplyCode.RPL_ENDOFSTATS:
                OnStats?.Invoke(this, new StatsEventArgs(
                    Rfc2812Util.CharToStatsQuery(tokens[3][0]),
                    RemoveLeadingColon(CondenseStrings(tokens, 4)), true));
                break;
            default:
                HandleDefaultReply(code, tokens);
                break;
        }
    }

    public void ProcessNamesReply(string[] tokens, bool end)
    {
        if (end)
        {
            OnNames?.Invoke(this, new NamesEventArgs(tokens[3], new string[0], true));
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
            OnNames?.Invoke(this, new NamesEventArgs(tokens[4], users, false));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessNamesReply() channel={tokens[4]} count={numberOfUsers}");
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="code"></param>
    /// <param name="tokens"></param>
    private void HandleDefaultReply(ReplyCode code, string[] tokens)
    {
        if (code is >= ReplyCode.ERR_NOSUCHNICK and <= ReplyCode.ERR_USERSDONTMATCH)
        {
            OnError?.Invoke(this, new ErrorMessageEventArgs(code, CondenseStrings(tokens, 3)));
        }
        else
        {
            OnReply?.Invoke(this, new ReplyEventArgs(code, CondenseStrings(tokens, 3)));
        }
    }
    /// <summary>
    /// Find the correct WhoIs object based on the nick name.
    /// </summary>
    /// <param name="nick"></param>
    /// <returns></returns>
    private WhoisInfo LookupInfo(string nick)
    {
        whoisInfos ??= new Hashtable();

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
            return string.Join(" ", strings, start, strings.Length - start);
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
    private string RemoveTrailingChar(string text) => text.Substring(0, text.Length - 1);

    private StatsQuery GetQueryType(ReplyCode code)
    {
        return code switch
        {
            ReplyCode.RPL_STATSLINKINFO => StatsQuery.Connections,
            ReplyCode.RPL_STATSCOMMANDS => StatsQuery.CommandUsage,
            ReplyCode.RPL_STATSUPTIME => StatsQuery.Uptime,
            ReplyCode.RPL_STATSOLINE => StatsQuery.Operators,
            //Should never get here
            _ => StatsQuery.CommandUsage,
        };
    }

    public IrcMessage ParseIrcMessage(string message)
    {
        var msg = new IrcMessage();

        string[] tokens = message.Split(Separator);
        msg.Tokens = tokens;

        msg.From = RemoveLeadingColon(tokens[0]);
        msg.Command = tokens[1];
        if (tokens.Length >= 3)
        {
            msg.Target = tokens[2];
        }

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
        OnNames?.Invoke(this, new NamesEventArgs(tokens[4], users, false));
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessNamesReply() channel={tokens[4]} count={numberOfUsers}");
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

    public void ProcessNoticeCommand(IrcMessage ircMessage)
    {
        var fromUser = Rfc2812Util.UserFromString(ircMessage.From);
        if (Rfc2812Util.IsValidChannelName(ircMessage.Target))
        {
            OnPublicNotice?.Invoke(this, new UserChannelMessageEventArgs(
                fromUser,
                ircMessage.Target,
                ircMessage.Message));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessNoticeCommand() public channel={ircMessage.Target}");
        }
        else
        {
            OnPrivateNotice?.Invoke(this, new UserMessageEventArgs(fromUser, ircMessage.Message));
            Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
                $"[{Thread.CurrentThread.Name}] Listener::ProcessNoticeCommand() private");
        }
    }

    public void ProcessInviteCommand(IrcMessage ircMessage)
    {
        Debug.WriteLineIf(Rfc2812Util.IrcTrace.TraceVerbose,
            $"[{Thread.CurrentThread.Name}] Listener::ProcessInviteCommand() channel={ircMessage.Message}");
        var fromUser = Rfc2812Util.UserFromString(ircMessage.From);
        OnInvite?.Invoke(this, new InviteEventArgs(fromUser.Nick, ircMessage.Message));
    }
}
