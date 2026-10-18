using System;
using System.Collections.Generic;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests;

[TestFixture]
public class ListenerTests
{
    private Listener _listener;
    private static readonly string Pong =
        ":hitchcock.freenode.net PONG hitchcock.freenode.net :ping";
    private static readonly string Names =
        ":hitchcock.freenode.net 353 OrtzIRC = #ortzirc :Ortzinator OrtzIRC @ChanServ";
    private static readonly string Join =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com JOIN #ortzirc";
    private static readonly string NoticePrivate =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com NOTICE OrtzIRC :foobar";
    private static readonly string Privmsg =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PRIVMSG #ortzirc :foobar";
    private static readonly string PrivmsgAction =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PRIVMSG #ortzirc :\u0001ACTION foobars\u0001";
    private static readonly string Nick =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com NICK :Ortz";
    private static readonly string Kick =
        ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com KICK #ortzirc OrtzIRC :OrtzIRC";

    private static readonly string Topic =
        ":hitchcock.freenode.net 332 Ortzinator #lancer :foobar";

    private static readonly string UserString =
        "Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com";
    private static readonly User TestUser = Rfc2812Util.UserFromString(UserString);

    [OneTimeSetUp]
    public void SetupMethods()
    {
    }

    [OneTimeTearDown]
    public void TearDownMethods()
    {
    }

    [SetUp]
    public void SetupTest()
    {
        _listener = new Listener();
    }

    [TearDown]
    public void TearDownTest()
    {
        _listener = null;
    }

    [Test]
    public void ParseIrcMessage_PongMessage()
    {
        IrcMessage msg = _listener.ParseIrcMessage(Pong);
        Assert.AreEqual("PONG", msg.Command);
        Assert.AreEqual("hitchcock.freenode.net", msg.From);
        Assert.AreEqual(ReplyCode.Null, msg.ReplyCode);
    }

    [Test]
    public void ParseIrcMessage_NoticeMessage()
    {
        IrcMessage msg = _listener.ParseIrcMessage(NoticePrivate);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("NOTICE", msg.Command);
        Assert.AreEqual("foobar", msg.Message);
    }

    [Test]
    public void ParseIrcMessage_NamesMessage()
    {
        IrcMessage msg = _listener.ParseIrcMessage(Names);
        Assert.AreEqual("hitchcock.freenode.net", msg.From);
        Assert.AreEqual(ReplyCode.RPL_NAMREPLY, msg.ReplyCode);
        Assert.AreEqual("Ortzinator OrtzIRC @ChanServ", msg.Message);
    }

    [Test]
    public void RemoveLeadingColon()
    {
        Assert.AreEqual("foobar", _listener.RemoveLeadingColon(":foobar"));
        Assert.AreEqual("foobar", _listener.RemoveLeadingColon("foobar"));
    }

    [Test]
    public void ProcessNamesReply_IrcMessage()
    {
        IrcMessage msg = new IrcMessage();
        msg.Command = "PRIVMSG";
        msg.From = "hitchcock.freenode.net";
        msg.Message = "Ortzinator OrtzIRC @ChanServ";
        msg.Target = "#ortzirc";
        msg.Tokens = Names.Split(new[] { ' ' });
        msg.ReplyCode = ReplyCode.RPL_NAMREPLY;

        NamesEventArgs givenArgs = null;

        _listener.OnNames += delegate (object sender, NamesEventArgs args) { givenArgs = args; };
        _listener.ProcessNamesReply(msg);
        Assert.AreEqual(new[] { "Ortzinator", "OrtzIRC", "@ChanServ" }, givenArgs.Nicks);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual(false, givenArgs.IsLast);
    }

    [Test]
    public void ParseIrcMessage_Privmsg()
    {
        IrcMessage msg = _listener.ParseIrcMessage(Privmsg);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("foobar", msg.Message);
        Assert.AreEqual("#ortzirc", msg.Target);
    }

    [Test]
    public void ParseIrcMessage_PrivmsgAction()
    {
        IrcMessage msg = _listener.ParseIrcMessage(PrivmsgAction);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("\u0001ACTION foobars\u0001", msg.Message);
        Assert.AreEqual("#ortzirc", msg.Target);
    }

    [Test]
    public void ProcessPrivmsgCommand_ActionMessage()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "PRIVMSG",
            From = UserString,
            Message = "\u0001ACTION foobars\u0001",
            Target = "#ortzirc",
            Tokens = PrivmsgAction.Split(new[] { ' ' })
        };
        UserChannelMessageEventArgs givenArgs = null;

        _listener.OnAction += delegate (object sender, UserChannelMessageEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessPrivmsgCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual("foobars", givenArgs.Message);
    }

    [Test]
    public void ProcessPrivmsgCommand_PrivateAction()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "PRIVMSG",
            From = UserString,
            Message = "\u0001ACTION foobars\u0001",
            Target = "Buster",
            Tokens = Privmsg.Split(new[] { ' ' })
        };
        UserMessageEventArgs givenArgs = null;

        _listener.OnPrivateAction += delegate (object sender, UserMessageEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessPrivmsgCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("foobars", givenArgs.Message);
    }

    [Test]
    public void ProcessPrivmsgCommand_PublicMessage()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "PRIVMSG",
            From = UserString,
            Message = "foobar",
            Target = "#ortzirc",
            Tokens = Privmsg.Split(new[] { ' ' })
        };
        UserChannelMessageEventArgs givenArgs = null;

        _listener.OnPublic += delegate (object sender, UserChannelMessageEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessPrivmsgCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual("foobar", givenArgs.Message);
    }

    [Test]
    public void ProcessPrivmsgCommand_PrivateMessage()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "PRIVMSG",
            From = UserString,
            Message = "foobar",
            Target = "Buster",
            Tokens = Privmsg.Split(new[] { ' ' })
        };
        UserMessageEventArgs givenArgs = null;

        _listener.OnPrivate += delegate (object sender, UserMessageEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessPrivmsgCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("foobar", givenArgs.Message);
    }

    [Test]
    public void CleanActionMessage()
    {
        Assert.AreEqual("foobars", _listener.CleanActionMessage("\u0001ACTION foobars\u0001"));
    }

    [Test]
    public void ParseIrcMessage_PrivateNotice()
    {
        IrcMessage msg = _listener.ParseIrcMessage(NoticePrivate);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("foobar", msg.Message);
        Assert.AreEqual(NoticePrivate.Split(new[] { ' ' }), msg.Tokens);
        Assert.AreEqual("OrtzIRC", msg.Target);
    }

    [Test]
    public void ParseIrcMessage_JoinCommand()
    {
        IrcMessage msg = _listener.ParseIrcMessage(Join);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("#ortzirc", msg.Target);
    }

    [Test]
    public void ParseIrcMessage_KickCommand()
    {
        IrcMessage msg = _listener.ParseIrcMessage(Kick);
        Assert.AreEqual(UserString, msg.From);
        Assert.AreEqual("#ortzirc", msg.Target);
    }

    [Test]
    public void ProcessNoticeCommand_PrivateNotice()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "NOTICE",
            From = UserString,
            Message = "foobar",
            Target = "OrtzIRC",
            Tokens = Privmsg.Split(new[] { ' ' })
        };
        UserMessageEventArgs givenArgs = null;

        _listener.OnPrivateNotice += delegate (object sender, UserMessageEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessNoticeCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("foobar", givenArgs.Message);
    }

    [Test]
    public void ProcessNickCommand()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "NICK",
            From = UserString,
            Message = "Ortz",
            Tokens = Nick.Split(new[] { ' ' })
        };
        NickChangeEventArgs givenArgs = null;
        _listener.OnNick += delegate (object sender, NickChangeEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessNickCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("Ortz", givenArgs.NewNick);
    }

    [Test]
    public void ProcessInviteCommand()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "INVITE",
            From = UserString,
            Message = "#ortzirc"
        };
        InviteEventArgs givenArgs = null;
        _listener.OnInvite += delegate (object sender, InviteEventArgs args)
        {
            givenArgs = args;
        };
        _listener.ProcessInviteCommand(msg);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual("Ortzinator", givenArgs.Nick);
    }

    [Test]
    public void ProcessJoinCommand()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "JOIN",
            From = UserString,
            Target = "#ortzirc"
        };
        JoinEventArgs givenArgs = null;
        _listener.OnJoin += (_, e) => givenArgs = e;
        _listener.ProcessJoinCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);

    }

    [Test]
    public void ProcessJoinCommand_TargetWithLeadingColon_StripsColon()
    {
        // Real servers send "JOIN :#channel"; ParseIrcMessage sets Target = ":#channel" (colon not stripped).
        // ProcessJoinCommand must strip it so CreateChannel receives a valid channel name.
        IrcMessage msg = new IrcMessage
        {
            Command = "JOIN",
            From = UserString,
            Target = ":#ortzirc"
        };
        string receivedChannel = null;
        _listener.OnJoin += (_, e) => receivedChannel = e.Channel;
        _listener.ProcessJoinCommand(msg);
        Assert.AreEqual("#ortzirc", receivedChannel);
    }

    [Test]
    public void ProcessKickCommand()
    {
        IrcMessage msg = new IrcMessage
        {
            Command = "KICK",
            From = UserString,
            Target = "OrtzIRC",
            Message = "noob",
            Tokens = Kick.Split(new[] { ' ' })
        };
        KickEventArgs givenArgs = null;
        _listener.OnKick += (_, e) => givenArgs = e;
        _listener.ProcessKickCommand(msg);
        Assert.AreEqual(TestUser, givenArgs.Kicker);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual("OrtzIRC", givenArgs.Kickee);
        Assert.AreEqual("noob", givenArgs.Reason);
    }

    [Test]
    public void Parse_Ping_OnPingFires()
    {
        string givenMsg = String.Empty;
        _listener.OnPing += (_, e) => givenMsg = e.Message;
        _listener.Parse("PING :irc.funet.fi");
        Assert.AreEqual("irc.funet.fi", givenMsg);
    }

    [Test]
    public void Parse_Notice_OnPrivateNoticeFires()
    {
        string givenMsg = String.Empty;
        _listener.OnPrivateNotice += delegate (object sender, UserMessageEventArgs args)
        {
            givenMsg = args.Message;
        };
        _listener.Parse("NOTICE OrtzIRC :foobar");
        Assert.AreEqual("foobar", givenMsg);
    }

    [Test]
    public void Parse_Error_OnErrorFires()
    {
        string givenMsg = String.Empty;
        _listener.OnError += delegate (object sender, ErrorMessageEventArgs args)
        {
            givenMsg = args.Message;
        };
        _listener.Parse("ERROR :the end is nigh");
        Assert.AreEqual("the end is nigh", givenMsg);
    }

    [Test]
    public void ParseReply_Topic()
    {
        string givenChannel = string.Empty;
        string givenTopic = string.Empty;
        _listener.OnReceiveTopic += (_, e) =>
        {
            givenChannel = e.Channel;
            givenTopic = e.Topic;
        };
        _listener.ParseReply(Topic.Split(new[] { ' ' }));
        Assert.That(givenChannel, Is.EqualTo("#lancer"));
        Assert.That(givenTopic, Is.EqualTo("foobar"));
    }

    [Test]
    public void Parse_ChannelMode_FiresOnChannelModeChange()
    {
        ChannelModeChangeEventArgs givenArgs = null;
        _listener.OnChannelModeChange += (_, e) => givenArgs = e;
        _listener.Parse(":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com MODE #ortzirc +m");
        Assert.AreEqual(TestUser, givenArgs.Changer);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual(1, givenArgs.Modes.Count);
        Assert.AreEqual(ModeAction.Add, givenArgs.Modes[0].Action);
        Assert.AreEqual(ChannelMode.Moderated, givenArgs.Modes[0].Mode);
        Assert.AreEqual("+m", givenArgs.Raw);
    }

    [Test]
    public void Parse_UserMode_FiresOnUserModeChange()
    {
        UserModeChangeEventArgs givenArgs = null;
        _listener.OnUserModeChange += (sender, args) => givenArgs = args;
        _listener.Parse(
            ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com MODE Ortzinator +i");
        Assert.IsNotNull(givenArgs);
        Assert.AreEqual(ModeAction.Add, givenArgs.Action);
        Assert.AreEqual(UserMode.Invisible, givenArgs.Mode);
    }

    [Test]
    public void Parse_Quit_FiresOnQuit()
    {
        User givenUser = null;
        string givenReason = null;
        _listener.OnQuit += (_, e) =>
        {
            givenUser = e.User;
            givenReason = e.Reason;
        };
        _listener.Parse(
            ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com QUIT :Quit: Leaving");
        Assert.AreEqual(TestUser, givenUser);
        Assert.AreEqual("Quit: Leaving", givenReason);
    }

    [Test]
    public void Parse_Quit_NoMessage_ReasonIsEmpty()
    {
        User givenUser = null;
        string givenReason = null;
        _listener.OnQuit += (_, e) =>
        {
            givenUser = e.User;
            givenReason = e.Reason;
        };
        _listener.Parse(":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com QUIT");
        Assert.AreEqual(TestUser, givenUser);
        Assert.AreEqual("", givenReason);
    }

    [Test]
    public void Parse_Part_FiresOnPart()
    {
        User givenUser = null;
        string givenChannel = null;
        string givenReason = null;
        _listener.OnPart += (_, e) =>
        {
            givenUser = e.User;
            givenChannel = e.Channel;
            givenReason = e.Reason;
        };
        _listener.Parse(
            ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PART #ortzirc :Goodbye");
        Assert.AreEqual(TestUser, givenUser);
        Assert.AreEqual("#ortzirc", givenChannel);
        Assert.AreEqual("Goodbye", givenReason);
    }

    [Test]
    public void Parse_Part_NoMessage_ReasonIsEmpty()
    {
        User givenUser = null;
        string givenChannel = null;
        string givenReason = null;
        _listener.OnPart += (_, e) =>
        {
            givenUser = e.User;
            givenChannel = e.Channel;
            givenReason = e.Reason;
        };
        _listener.Parse(":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PART #ortzirc");
        Assert.AreEqual(TestUser, givenUser);
        Assert.AreEqual("#ortzirc", givenChannel);
        Assert.AreEqual("", givenReason);
    }

    [Test]
    public void Parse_Topic_FiresOnTopicChanged()
    {
        UserChannelMessageEventArgs givenArgs = null;
        _listener.OnTopicChanged += (sender, args) => givenArgs = args;
        _listener.Parse(
            ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com TOPIC #ortzirc :New topic here");
        Assert.AreEqual(TestUser, givenArgs.User);
        Assert.AreEqual("#ortzirc", givenArgs.Channel);
        Assert.AreEqual("New topic here", givenArgs.Message);
    }

    [Test]
    public void Parse_Kill_FiresOnKill()
    {
        User givenUser = null;
        string givenNick = null;
        string givenReason = null;
        _listener.OnKill += (_, e) =>
        {
            givenUser = e.Killer;
            givenNick = e.Nick;
            givenReason = e.Reason;
        };
        _listener.Parse(":irc.server.net KILL Ortzinator :Killed by operator");
        Assert.AreEqual("irc.server.net", givenUser.Nick);
        Assert.AreEqual("Ortzinator", givenNick);
        Assert.AreEqual("Killed by operator", givenReason);
    }

    [Test]
    public void Parse_Kill_NoMessage_ReasonIsEmpty()
    {
        User givenUser = null;
        string givenNick = null;
        string givenReason = null;
        _listener.OnKill += (_, e) =>
        {
            givenUser = e.Killer;
            givenNick = e.Nick;
            givenReason = e.Reason;
        };
        _listener.Parse(":irc.server.net KILL Ortzinator");
        Assert.AreEqual("Ortzinator", givenNick);
        Assert.AreEqual("", givenReason);
    }

    [Test]
    public void Parse_Away_FiresOnAway()
    {
        AwayEventArgs givenArgs = null;
        _listener.OnAway += (sender, args) => givenArgs = args;
        _listener.Parse(":irc.server.net 301 OrtzIRC Buster :Gone fishing");
        Assert.IsNotNull(givenArgs);
        Assert.AreEqual("Buster", givenArgs.Nick);
        Assert.AreEqual("Gone fishing", givenArgs.AwayMessage);
    }

    [Test]
    public void Parse_NowAway_FiresOnNowAway()
    {
        bool fired = false;
        _listener.OnNowAway += (sender, args) => fired = true;
        _listener.Parse(":irc.server.net 306 OrtzIRC :You have been marked as being away");
        Assert.IsTrue(fired);
    }

    [Test]
    public void Parse_UnAway_FiresOnUnAway()
    {
        bool fired = false;
        _listener.OnUnAway += (sender, args) => fired = true;
        _listener.Parse(":irc.server.net 305 OrtzIRC :You are no longer marked as being away");
        Assert.IsTrue(fired);
    }

    [Test]
    public void Parse_List_FiresOnListForEachChannelThenLast()
    {
        var given = new List<ListEventArgs>();
        _listener.OnList += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 322 OrtzIRC #ortzirc 12 :All about IRC");
        _listener.Parse(":irc.server.net 323 OrtzIRC :End of /LIST");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual("#ortzirc", given[0].Channel);
        Assert.AreEqual(12, given[0].VisibleNickCount);
        Assert.AreEqual("All about IRC", given[0].Topic);
        Assert.IsFalse(given[0].IsLast);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Ison_FiresOnIson()
    {
        IsonEventArgs givenArgs = null;
        _listener.OnIson += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 303 OrtzIRC Buster");
        Assert.AreEqual("Buster", givenArgs.Nicks);
    }

    [Test]
    public void Parse_Who_FiresOnWhoForEachUserThenLast()
    {
        var given = new List<WhoEventArgs>();
        _listener.OnWho += (_, e) => given.Add(e);
        _listener.Parse(
            ":irc.server.net 352 OrtzIRC #ortzirc ~buster buster.host hub.server.net Buster H@ :3 Buster");
        _listener.Parse(":irc.server.net 315 OrtzIRC #ortzirc :End of /WHO list.");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual("Buster", given[0].User.Nick);
        Assert.AreEqual("#ortzirc", given[0].Channel);
        Assert.AreEqual("hub.server.net", given[0].IrcServer);
        Assert.AreEqual("H@", given[0].Mask);
        Assert.AreEqual(3, given[0].HopCount);
        Assert.AreEqual("Buster", given[0].RealName);
        Assert.IsFalse(given[0].IsLast);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Whois_FiresOnWhoisWithTheCollectedReplies()
    {
        WhoisEventArgs givenArgs = null;
        _listener.OnWhois += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 311 OrtzIRC Buster ~buster buster.host * :Buster Bluth");
        _listener.Parse(":irc.server.net 312 OrtzIRC Buster hub.server.net :The hub");
        _listener.Parse(":irc.server.net 317 OrtzIRC Buster 42 1700000000 :seconds idle");
        _listener.Parse(":irc.server.net 318 OrtzIRC Buster :End of /WHOIS list.");
        Assert.AreEqual("Buster", givenArgs.WhoisInfo.User.Nick);
        Assert.AreEqual("Buster Bluth", givenArgs.WhoisInfo.RealName);
        Assert.AreEqual("hub.server.net", givenArgs.WhoisInfo.Server);
        Assert.AreEqual("The hub", givenArgs.WhoisInfo.ServerDescription);
    }

    [Test]
    public void Parse_Whowas_FiresOnWhowasThenLast()
    {
        var given = new List<WhowasEventArgs>();
        _listener.OnWhowas += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 314 OrtzIRC Buster ~buster buster.host * :Buster Bluth");
        _listener.Parse(":irc.server.net 369 OrtzIRC Buster :End of WHOWAS");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual("Buster", given[0].User.Nick);
        Assert.AreEqual("Buster Bluth", given[0].RealName);
        Assert.IsFalse(given[0].IsLast);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_UserModeReply_FiresOnUserModeRequest()
    {
        UserModeRequestEventArgs givenArgs = null;
        _listener.OnUserModeRequest += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 221 OrtzIRC +iw");
        Assert.That(givenArgs.Modes, Is.EqualTo(new[] { UserMode.Invisible, UserMode.Wallops }));
    }

    [Test]
    public void Parse_BanList_FiresOnChannelListThenLast()
    {
        var given = new List<ChannelListEventArgs>();
        _listener.OnChannelList += (_, e) => given.Add(e);
        _listener.Parse(
            ":irc.server.net 367 OrtzIRC #ortzirc *!*@bad.host Ortzinator!~ortz@host 1700000000");
        _listener.Parse(":irc.server.net 368 OrtzIRC #ortzirc :End of channel ban list");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual("#ortzirc", given[0].Channel);
        Assert.AreEqual(ChannelMode.Ban, given[0].Mode);
        Assert.AreEqual("*!*@bad.host", given[0].Item);
        Assert.AreEqual("Ortzinator", given[0].Setter.Nick);
        Assert.AreEqual(1700000000L, given[0].WhenSet);
        Assert.IsFalse(given[0].IsLast);
        Assert.AreEqual(ChannelMode.Ban, given[1].Mode);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Version_FiresOnVersion()
    {
        VersionEventArgs givenArgs = null;
        _listener.OnVersion += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 351 OrtzIRC ircd-1.0.0 irc.server.net :Some comments");
        Assert.AreEqual("ircd-1.0.0 irc.server.net :Some comments", givenArgs.VersionInfo);
    }

    [Test]
    public void Parse_Motd_FiresOnMotdForEachLineThenLast()
    {
        var given = new List<MotdEventArgs>();
        _listener.OnMotd += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 372 OrtzIRC :- Welcome aboard");
        _listener.Parse(":irc.server.net 376 OrtzIRC :End of /MOTD command.");
        Assert.AreEqual(2, given.Count);
        Assert.That(given[0].Message, Does.EndWith("- Welcome aboard"));
        Assert.IsFalse(given[0].IsLast);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Time_FiresOnTime()
    {
        TimeEventArgs givenArgs = null;
        _listener.OnTime += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 391 OrtzIRC irc.server.net :Tuesday October 14 2026");
        Assert.That(givenArgs.Time, Does.StartWith("irc.server.net"));
        Assert.That(givenArgs.Time, Does.EndWith("Tuesday October 14 2026"));
    }

    [Test]
    public void Parse_Info_FiresOnInfoForEachLineThenLast()
    {
        var given = new List<InfoEventArgs>();
        _listener.OnInfo += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 371 OrtzIRC :Written in C");
        _listener.Parse(":irc.server.net 374 OrtzIRC :End of /INFO list.");
        Assert.AreEqual(2, given.Count);
        Assert.That(given[0].Message, Does.EndWith("Written in C"));
        Assert.IsFalse(given[0].IsLast);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Admin_FiresOnAdmin()
    {
        AdminEventArgs givenArgs = null;
        _listener.OnAdmin += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 257 OrtzIRC :Helsinki, Finland");
        Assert.AreEqual("Helsinki, Finland", givenArgs.Message);
    }

    [Test]
    public void Parse_Lusers_FiresOnLusers()
    {
        LusersEventArgs givenArgs = null;
        _listener.OnLusers += (_, e) => givenArgs = e;
        _listener.Parse(":irc.server.net 251 OrtzIRC :There are 5 users on 1 server");
        Assert.AreEqual("There are 5 users on 1 server", givenArgs.Message);
    }

    [Test]
    public void Parse_Links_FiresOnLinksForEachServerThenLast()
    {
        var given = new List<LinksEventArgs>();
        _listener.OnLinks += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 364 OrtzIRC *.server.net hub.server.net :1 Example Net");
        _listener.Parse(":irc.server.net 365 OrtzIRC * :End of /LINKS list.");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual("*.server.net", given[0].Mask);
        Assert.AreEqual("hub.server.net", given[0].Hostname);
        Assert.AreEqual(1, given[0].HopCount);
        Assert.AreEqual("Example Net", given[0].ServerInfo);
        Assert.IsFalse(given[0].IsLast);
        Assert.AreEqual(-1, given[1].HopCount);
        Assert.IsTrue(given[1].IsLast);
    }

    [Test]
    public void Parse_Stats_FiresOnStatsThenLast()
    {
        var given = new List<StatsEventArgs>();
        _listener.OnStats += (_, e) => given.Add(e);
        _listener.Parse(":irc.server.net 243 OrtzIRC O *@oper.host * Buster");
        _listener.Parse(":irc.server.net 219 OrtzIRC o :End of /STATS report");
        Assert.AreEqual(2, given.Count);
        Assert.AreEqual(StatsQuery.Operators, given[0].QueryType);
        Assert.AreEqual("O *@oper.host * Buster", given[0].Message);
        Assert.IsFalse(given[0].IsLast);
        Assert.AreEqual(StatsQuery.Operators, given[1].QueryType);
        Assert.AreEqual("End of /STATS report", given[1].Message);
        Assert.IsTrue(given[1].IsLast);
    }
}
