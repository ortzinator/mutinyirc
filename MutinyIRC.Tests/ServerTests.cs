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
        [ExpectedException(typeof(ArgumentException))]
        public void SetupConnection_GivenConnectionArgsAndNickNotSet_ArgumentExceptionThrown()
        {
            _server.SetupConnection(new ConnectionArgs());
        }
    }
}
