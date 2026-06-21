using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Builds a <see cref="Server"/> backed by a fake <see cref="IConnection"/> so tests can
    ///   control <c>Connected</c>, carry a known <c>Url</c> (the log-file key), and exercise
    ///   teardown without opening a socket.
    /// </summary>
    internal static class ServerTestFactory
    {
        /// <summary>
        ///   <paramref name="connected"/> drives <see cref="Server.IsConnected"/>. The
        ///   <c>Listener</c> is stubbed to a real instance because <see cref="Server.Disconnect(string)"/>
        ///   -&gt; <c>UnhookEvents</c> detaches handlers from <c>Connection.Listener</c>, which would
        ///   NRE against a fake's default null.
        /// </summary>
        public static Server FakeServer(string host = "irc.fake.com", bool connected = false)
        {
            var conn = A.Fake<IConnection>();
            A.CallTo(() => conn.Connected).Returns(connected);
            A.CallTo(() => conn.Listener).Returns(new Listener());
            A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("nick", host, false));
            return new Server { Connection = conn };
        }
    }
}
