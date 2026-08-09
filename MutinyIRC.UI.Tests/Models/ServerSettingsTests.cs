using System.Linq;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

[TestFixture]
public class ServerSettingsTests
{
    private static ServerSettings WithPorts(string ports) =>
        new("irc.libera.chat", "Libera", ports, false);

    // ── RandomPort ───────────────────────────────────────────────────────────

    [Test]
    public void RandomPort_OverManyDraws_ReachesEveryPort()
    {
        // Every configured port must be reachable. An exclusive upper bound made the
        // last one unreachable. 300 draws over 3 ports: a miss is about 1 in 10^52.
        var server = WithPorts("6667,6668,7000");

        var seen = Enumerable.Range(0, 300)
            .Select(_ => server.RandomPort)
            .Distinct();

        Assert.That(seen, Is.EquivalentTo(new[] { 6667, 6668, 7000 }));
    }

    [Test]
    public void RandomPort_OverManyDraws_ReachesEveryPortInARange()
    {
        var server = WithPorts("6667-6669");

        var seen = Enumerable.Range(0, 300)
            .Select(_ => server.RandomPort)
            .Distinct();

        Assert.That(seen, Is.EquivalentTo(new[] { 6667, 6668, 6669 }));
    }

    [Test]
    public void RandomPort_OnePort_ReturnsIt()
    {
        var server = WithPorts("6667");

        Assert.That(server.RandomPort, Is.EqualTo(6667));
    }

    [Test]
    public void RandomPort_NoPorts_ThrowsInvalidOperation()
    {
        var server = WithPorts(string.Empty);

        Assert.That(() => server.RandomPort, Throws.InvalidOperationException);
    }
}
