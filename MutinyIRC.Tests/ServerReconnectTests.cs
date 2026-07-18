using System.Collections.Generic;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests;

/// <summary>
/// Pins the Server's automatic-reconnect policy: after a non-user-initiated drop it schedules
/// a reconnect and announces the delay via <see cref="Server.Reconnecting"/>, with the delay
/// following exponential backoff (doubling from a 4s base, capped at 5 minutes) and resetting
/// once a registration succeeds. The <c>Reconnecting</c> event fires synchronously inside the
/// drop handler — before the timer that actually reconnects — so the announced delays can be
/// asserted without waiting on the clock.
///
/// Each test disposes its Server (via <c>using</c>) so the last scheduled timer is cancelled
/// rather than firing a real connect attempt against the fake host after the test returns.
/// </summary>
[TestFixture]
public class ServerReconnectTests
{
    private static Connection OfflineConnection() =>
        new Connection(new ConnectionArgs("test", "irc.fake.com", false), false, false);

    private static List<double> CaptureReconnectDelays(Server server)
    {
        var delays = new List<double>();
        server.Reconnecting += (_, e) => delays.Add(e.Data.TotalSeconds);
        return delays;
    }

    [Test]
    public void RepeatedDrops_ScheduleReconnectsWithExponentialBackoff()
    {
        var conn = OfflineConnection();
        using var server = new Server(conn);
        var delays = CaptureReconnectDelays(server);

        for (int i = 0; i < 3; i++)
            conn.Disconnect(DisconnectReason.SocketError);

        CollectionAssert.AreEqual(new[] { 4d, 8d, 16d }, delays,
            "Each consecutive drop must double the reconnect delay from the 4s base.");
    }

    [Test]
    public void Backoff_SaturatesAtMaxDelay()
    {
        var conn = OfflineConnection();
        using var server = new Server(conn);
        var delays = CaptureReconnectDelays(server);

        // Far more drops than it takes to exceed the cap (4s doubling reaches 5 min by the 7th).
        for (int i = 0; i < 20; i++)
            conn.Disconnect(DisconnectReason.SocketError);

        Assert.AreEqual(300d, delays[^1],
            "The backoff must saturate at the 5-minute cap rather than growing without bound.");
    }

    [Test]
    public void SuccessfulRegistration_ResetsBackoff()
    {
        var conn = OfflineConnection();
        using var server = new Server(conn);
        var delays = CaptureReconnectDelays(server);

        conn.Disconnect(DisconnectReason.SocketError); // 4
        conn.Disconnect(DisconnectReason.SocketError); // 8
        conn.Listener.Parse(":server.name 001 test :Welcome"); // RPL_WELCOME -> Registered
        conn.Disconnect(DisconnectReason.SocketError); // back to 4

        CollectionAssert.AreEqual(new[] { 4d, 8d, 4d }, delays,
            "A successful registration must reset the backoff so the next drop starts at the base.");
    }

    [Test]
    public void UserInitiatedDisconnect_DoesNotScheduleAReconnect()
    {
        var conn = OfflineConnection();
        using var server = new Server(conn);
        var delays = CaptureReconnectDelays(server);

        conn.Disconnect(DisconnectReason.UserInitiated);

        CollectionAssert.IsEmpty(delays,
            "A clean, user-initiated quit must not trigger an automatic reconnect.");
    }
}
