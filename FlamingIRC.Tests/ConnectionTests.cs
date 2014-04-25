using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FakeItEasy;

namespace FlamingIRC.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        private Connection _connection;
        private ConnectionArgs _connectionArgs;

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
            _connectionArgs = new ConnectionArgs();
            _connection = new Connection(_connectionArgs, false, false);
        }

        [TearDown]
        public void TearDownTest()
        {
            _connection = null;
        }

        [Test]
        public void OnReceiveLine_()
        {
        }
    }
}