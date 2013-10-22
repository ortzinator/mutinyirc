using System;
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
            _server = new Server();
        }

        [TearDown]
        public void Teardown()
        {
            _server = null;
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetupConnection_NickNotSet_ArgumentExceptionThrown()
        {
            _server.SetupConnection(new ConnectionArgs());
        }
    }
}
