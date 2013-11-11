using System;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using OrtzIRC.Common;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private Server _server;

        [SetUp]
        public void Setup()
        {
            var args = new ConnectionArgs("test", "irc.fake.com", false);
            var connMock = A.Fake<Connection>(x => x.WithArgumentsForConstructor(() => new Connection(args, false, false)));
            _server = new Server(connMock);
        }

        [TearDown]
        public void Teardown()
        {
            _server = null;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetupConnection_GivenConnectionArgsAndNickNotSet_ArgumentNullExceptionThrown()
        {
            _server.SetupConnection(new ConnectionArgs());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Connection_SetToNull_ArgumentNullExceptionThrown()
        {
            _server.Connection = null;
        }
    }
}
