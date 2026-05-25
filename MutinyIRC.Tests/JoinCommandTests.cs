using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Commands;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.Tests
{
    [TestFixture]
    public class JoinCommandTests
    {
        [Test]
        public void Execute_WithServerContextAndKey_SendsJoinLineIncludingKey()
        {
            var args = new ConnectionArgs("test", "irc.fake.com", false);
            var connection = A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
            var server = new Server(connection);
            string sent = null;
            connection.RawMessageSent += (_, e) => sent = e.Data;

            new Join().Execute(server, new ChannelInfo("#mutiny"), "secret");

            Assert.That(sent, Is.EqualTo("JOIN #mutiny secret"));
        }
    }
}