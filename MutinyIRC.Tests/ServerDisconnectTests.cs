using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Pins how the Server maps a FlamingIRC <c>ConnectionLost</c> to its own events, and that
    ///   the firing is <em>synchronous, inline</em>. A user-initiated loss surfaces as
    ///   <c>Disconnected</c>; any other reason surfaces as <c>ConnectionLost</c> (and not as
    ///   <c>Disconnected</c>) — the asymmetry the UI relies on to tell a clean quit from an
    ///   unexpected drop. The inline firing is what lets <see cref="Server.Disconnect(string)"/>
    ///   call <c>Connection.Disconnect</c> and then immediately <c>UnhookEvents()</c> while still
    ///   surfacing the "--Disconnected--" line: the event escapes during the first call, before
    ///   the second detaches the handler.
    ///
    ///   These exercise <c>Connection_ConnectionLost</c> directly via FlamingIRC's teardown rather
    ///   than through <see cref="Server.Disconnect(string)"/>: that path early-returns on an
    ///   unconnected socket (and would attempt a QUIT send), so it can't reach the mapping without
    ///   a live connection. They use a real <see cref="Connection"/> rather than a fake because the
    ///   behaviour under test is precisely that the real teardown fires the event inline — a fake
    ///   would let us assert nothing more than our own stub. Calling the
    ///   <see cref="DisconnectReason"/> overload reproduces the exact inline <c>ConnectionLost</c>
    ///   the socket teardown fires, without needing a live socket. If FlamingIRC ever deferred that
    ///   firing to a callback, the inline assertions below would see the events not yet raised and
    ///   fail.
    /// </summary>
    [TestFixture]
    public class ServerDisconnectTests
    {
        private static Connection OfflineConnection() =>
            new Connection(new ConnectionArgs("test", "irc.fake.com", false), false, false);

        [Test]
        public void Disconnect_UserInitiated_SurfacesAsDisconnectedSynchronously()
        {
            var conn = OfflineConnection();
            var server = new Server(conn); // ctor hooks ConnectionLost -> Disconnected mapping

            bool disconnectedFired = false;
            server.Disconnected += (_, _) => disconnectedFired = true;

            conn.Disconnect(DisconnectReason.UserInitiated);

            // Asserting right after the call (no waiting) is the point: the event must already
            // have fired, proving it is raised inline within the disconnect rather than deferred.
            Assert.IsTrue(disconnectedFired,
                "A user-initiated disconnect must surface as Server.Disconnected synchronously, " +
                "so it fires before Server.Disconnect unhooks the handler.");
        }

        [Test]
        public void Disconnect_NonUserInitiated_SurfacesAsConnectionLostNotDisconnected()
        {
            var conn = OfflineConnection();
            var server = new Server(conn);

            bool connectionLostFired = false;
            bool disconnectedFired = false;
            server.ConnectionLost += (_, _) => connectionLostFired = true;
            server.Disconnected += (_, _) => disconnectedFired = true;

            conn.Disconnect(DisconnectReason.SocketError);

            // The discriminating branch: anything other than UserInitiated is an unexpected drop,
            // which must surface as ConnectionLost so the UI can offer to reconnect — and must not
            // masquerade as the clean Disconnected line.
            Assert.IsTrue(connectionLostFired,
                "A non-user-initiated disconnect must surface as Server.ConnectionLost.");
            Assert.IsFalse(disconnectedFired,
                "A non-user-initiated disconnect must not surface as Disconnected — that line is " +
                "reserved for a clean, user-initiated quit.");
        }

        [Test]
        public void Disconnect_AfterUnhookEvents_DoesNotSurfaceAsDisconnected()
        {
            var conn = OfflineConnection();
            var server = new Server(conn);
            server.UnhookEvents();

            bool disconnectedFired = false;
            server.Disconnected += (_, _) => disconnectedFired = true;

            conn.Disconnect(DisconnectReason.UserInitiated);

            Assert.IsFalse(disconnectedFired,
                "Once UnhookEvents has detached the handler, a disconnect must no longer surface " +
                "as Disconnected — confirming the live mapping is exactly what UnhookEvents removes.");
        }
    }
}