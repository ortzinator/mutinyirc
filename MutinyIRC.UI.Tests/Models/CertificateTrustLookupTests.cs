using System.IO;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

/// <summary>
/// Covers how the saved networks answer the certificate trust lookup that a connection makes
/// at each TLS handshake: by the host, through the network that lists it.
/// </summary>
[TestFixture]
public class CertificateTrustLookupTests
{
    private static IrcSettingsManager ManagerWith(params NetworkSettings[] networks)
    {
        // A file of this fixture's own: the test must never reach the user's servers.json.
        var manager = new IrcSettingsManager(
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json"));
        manager.Networks.ReplaceAll(networks);
        return manager;
    }

    [Test]
    public void AHostOfANetworkWithTheSwitchOff_IsNotVerified()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true));

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("BOUNCER.home");

        Assert.That(trust.VerifyCertificates, Is.False);
    }

    [Test]
    public void AHostOfANetworkWithTheSwitchOn_IsVerified()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true));

        CertificateTrust trust = ManagerWith(libera).GetCertificateTrust("irc.libera.chat");

        Assert.That(trust.VerifyCertificates, Is.True);
    }

    [Test]
    public void AHostThatNoNetworkLists_IsVerified()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true));

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("irc.example.net");

        Assert.That(trust.VerifyCertificates, Is.True);
    }
}
