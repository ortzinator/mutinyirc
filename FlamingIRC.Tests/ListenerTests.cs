using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FakeItEasy;

namespace FlamingIRC.Tests
{
    [TestFixture]
    public class ListenerTests
    {
        private Listener _listener;
        private const string _pong = ":hitchcock.freenode.net PONG hitchcock.freenode.net :ping";
        private const string _names = ":hitchcock.freenode.net 353 OrtzIRC = #ortzirc :Ortzinator OrtzIRC @ChanServ";
        private const string _namesEnd = ":hitchcock.freenode.net 366 OrtzIRC #ortzirc :End of /NAMES list.";
        private const string _quit = ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com QUIT :Quit: Leaving";
        private const string _join = ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com JOIN #ortzirc";
        private const string _notice = ":hitchcock.freenode.net NOTICE * :*** Looking up your hostname...";
        private const string _privmsg = ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PRIVMSG #ortzirc :foobar";
        private const string _privmsgAction =
            ":Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com PRIVMSG #ortzirc :\u0001ACTION foobars\u0001";
        private const string _userString = "Ortzinator!~ortz@cpe-075-184-002-005.nc.res.rr.com";
        private readonly User _testUser = Rfc2812Util.UserFromString(_userString);

        [TestFixtureSetUp]
        public void SetupMethods()
        {
        }

        [TestFixtureTearDown]
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
            IrcMessage msg = _listener.ParseIrcMessage(_pong);
            Assert.AreEqual("PONG", msg.Command);
            Assert.AreEqual("hitchcock.freenode.net", msg.From);
            Assert.AreEqual(ReplyCode.Null, msg.ReplyCode);
        }

        [Test]
        public void ParseIrcMessage_NoticeMessage()
        {
            IrcMessage msg = _listener.ParseIrcMessage(_notice);
            Assert.AreEqual("hitchcock.freenode.net", msg.From);
            Assert.AreEqual("NOTICE", msg.Command);
            Assert.AreEqual("*** Looking up your hostname...", msg.Message);
        }

        [Test]
        public void ParseIrcMessage_NamesMessage()
        {
            IrcMessage msg = _listener.ParseIrcMessage(_names);
            Assert.AreEqual("hitchcock.freenode.net", msg.From);
            Assert.AreEqual(ReplyCode.RPL_NAMREPLY, msg.ReplyCode);
            Assert.AreEqual("Ortzinator OrtzIRC @ChanServ", msg.Message);
            Assert.AreEqual(_names.Split(new[] { ' ' }), msg.Tokens);
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
            IrcMessage msg = new IrcMessage
                             {
                                 Command = "PRIVMSG",
                                 From = "hitchcock.freenode.net",
                                 Message = "Ortzinator OrtzIRC @ChanServ",
                                 Target = "#ortzirc",
                                 Tokens = _names.Split(new[] { ' ' }),
                                 ReplyCode = ReplyCode.RPL_NAMREPLY
                             };
            NamesEventArgs givenArgs = null;

            _listener.OnNames += delegate(object sender, NamesEventArgs args) { givenArgs = args; };
            _listener.ProcessNamesReply(msg);
            Assert.AreEqual(new[] { "Ortzinator", "OrtzIRC", "@ChanServ" }, givenArgs.Nicks);
            Assert.AreEqual("#ortzirc", givenArgs.Channel);
            Assert.AreEqual(false, givenArgs.Last);
        }

        [Test]
        public void ParseIrcMessage_Privmsg()
        {
            IrcMessage msg = _listener.ParseIrcMessage(_privmsg);
            Assert.AreEqual(_userString, msg.From);
            Assert.AreEqual("foobar", msg.Message);
            Assert.AreEqual(_privmsg.Split(new[] { ' ' }), msg.Tokens);
            Assert.AreEqual("#ortzirc", msg.Target);
        }

        [Test]
        public void ParseIrcMessage_PrivmsgAction()
        {
            IrcMessage msg = _listener.ParseIrcMessage(_privmsgAction);
            Assert.AreEqual(_userString, msg.From);
            Assert.AreEqual("\u0001ACTION foobars\u0001", msg.Message);
            Assert.AreEqual(_privmsgAction.Split(new[] { ' ' }), msg.Tokens);
            Assert.AreEqual("#ortzirc", msg.Target);
        }

        [Test]
        public void ProcessPrivmsgCommand_ActionMessage()
        {
            IrcMessage msg = new IrcMessage
                             {
                                 Command = "PRIVMSG",
                                 From = _userString,
                                 Message = "\u0001ACTION foobars\u0001",
                                 Target = "#ortzirc",
                                 Tokens = _privmsgAction.Split(new[] { ' ' })
                             };
            UserChannelMessageEventArgs givenArgs = null;

            _listener.OnAction += delegate(object sender, UserChannelMessageEventArgs args) { givenArgs = args; };
            _listener.ProcessPrivmsgCommand(msg);
            Assert.AreEqual(_testUser, givenArgs.User);
            Assert.AreEqual("#ortzirc", givenArgs.Channel);
            Assert.AreEqual("foobars", givenArgs.Message);
        }

        [Test]
        public void ProcessPrivmsgCommand_PrivateAction()
        {
            IrcMessage msg = new IrcMessage
            {
                Command = "PRIVMSG",
                From = _userString,
                Message = "\u0001ACTION foobars\u0001",
                Target = "Buster",
                Tokens = _privmsg.Split(new[] { ' ' })
            };
            UserMessageEventArgs givenArgs = null;

            _listener.OnPrivateAction += delegate(object sender, UserMessageEventArgs args) { givenArgs = args; };
            _listener.ProcessPrivmsgCommand(msg);
            Assert.AreEqual(_testUser, givenArgs.User);
            Assert.AreEqual("foobars", givenArgs.Message);
        }

        [Test]
        public void ProcessPrivmsgCommand_PublicMessage()
        {
            IrcMessage msg = new IrcMessage
            {
                Command = "PRIVMSG",
                From = _userString,
                Message = "foobar",
                Target = "#ortzirc",
                Tokens = _privmsg.Split(new[] { ' ' })
            };
            UserChannelMessageEventArgs givenArgs = null;

            _listener.OnPublic += delegate(object sender, UserChannelMessageEventArgs args) { givenArgs = args; };
            _listener.ProcessPrivmsgCommand(msg);
            Assert.AreEqual(_testUser, givenArgs.User);
            Assert.AreEqual("#ortzirc", givenArgs.Channel);
            Assert.AreEqual("foobar", givenArgs.Message);
        }

        [Test]
        public void ProcessPrivmsgCommand_PrivateMessage()
        {
            IrcMessage msg = new IrcMessage
            {
                Command = "PRIVMSG",
                From = _userString,
                Message = "foobar",
                Target = "Buster",
                Tokens = _privmsg.Split(new[] { ' ' })
            };
            UserMessageEventArgs givenArgs = null;

            _listener.OnPrivate += delegate(object sender, UserMessageEventArgs args) { givenArgs = args; };
            _listener.ProcessPrivmsgCommand(msg);
            Assert.AreEqual(_testUser, givenArgs.User);
            Assert.AreEqual("foobar", givenArgs.Message);
        }
    }
}