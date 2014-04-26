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
            Assert.AreEqual(_names.Split(new []{' '}), msg.Tokens);
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
            IrcMessage msg = _listener.ParseIrcMessage(_names);
            NamesEventArgs expectedArgs = new NamesEventArgs("#ortzirc", new[] { "Ortzinator", "OrtzIRC", "@ChanServ" }, false);
            NamesEventArgs givenArgs = null;

            _listener.OnNames += delegate(object sender, NamesEventArgs args)
                                 {
                                     givenArgs = args;
                                 };
            _listener.ProcessNamesReply(msg);
            Assert.AreEqual(expectedArgs.Nicks, givenArgs.Nicks);
            Assert.AreEqual(expectedArgs.Channel, givenArgs.Channel);
            Assert.AreEqual(expectedArgs.Last, givenArgs.Last);
        }
    }
}