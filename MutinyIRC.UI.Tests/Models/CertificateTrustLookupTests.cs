using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MutinyIRC.Common;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Models;

/// <summary>
/// Covers how the saved networks answer the certificate trust lookup that a connection makes
/// at each TLS handshake. The answer holds the verify switch of the network that lists the host,
/// and the pin of the entry point (host and port). Also covers how a pin is saved on an entry point.
/// </summary>
[TestFixture]
public class CertificateTrustLookupTests
{
    private const string Fingerprint = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    private string _path = null!;

    // A file of this fixture's own: the test must never reach the user's servers.json.
    [SetUp]
    public void Setup() => _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

    [TearDown]
    public void Teardown() => File.Delete(_path);

    private IrcSettingsManager ManagerWith(params NetworkSettings[] networks)
    {
        var manager = new IrcSettingsManager(_path);
        manager.Networks.ReplaceAll(networks);
        return manager;
    }

    [Test]
    public void AHostOfANetworkWithTheSwitchOff_IsNotVerified()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true));

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("BOUNCER.home", 6697);

        Assert.That(trust.VerifyCertificates, Is.False);
    }

    [Test]
    public void AHostOfANetworkWithTheSwitchOn_IsVerified()
    {
        var libera = new NetworkSettings("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true));

        CertificateTrust trust = ManagerWith(libera).GetCertificateTrust("irc.libera.chat", 6697);

        Assert.That(trust.VerifyCertificates, Is.True);
    }

    [Test]
    public void AHostThatNoNetworkLists_IsVerifiedAndHasNoPin()
    {
        var home = new NetworkSettings("Home") { VerifyCertificates = false };
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("irc.example.net", 6697);

        Assert.That(trust, Is.EqualTo(CertificateTrust.Default));
    }

    [Test]
    public void AnEntryPointWithAPin_GivesItsPin()
    {
        var home = new NetworkSettings("Home");
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("Bouncer.Home", 6697);

        Assert.That(trust.PinnedFingerprint, Is.EqualTo(Fingerprint));
    }

    [Test]
    public void ThePinnedHostOnAnotherPort_HasNoPin()
    {
        var home = new NetworkSettings("Home");
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });

        CertificateTrust trust = ManagerWith(home).GetCertificateTrust("bouncer.home", 7000);

        Assert.Multiple(() =>
        {
            Assert.That(trust.PinnedFingerprint, Is.Null, "a pin belongs to one entry point: host and port");
            Assert.That(trust.VerifyCertificates, Is.True, "the host still belongs to its network");
        });
    }

    [Test]
    public void PinCertificate_PinsTheEntryPointAndSavesTheFile()
    {
        var home = new NetworkSettings("Home");
        home.AddServer(new ServerSettings("bouncer.home", 6697, true));
        IrcSettingsManager manager = ManagerWith(home);

        bool pinned = manager.PinCertificate("BOUNCER.home", 6697, Fingerprint);

        var saved = JsonSerializer.Deserialize<List<NetworkSettings>>(File.ReadAllText(_path))!;
        Assert.Multiple(() =>
        {
            Assert.That(pinned, Is.True);
            Assert.That(manager.GetCertificateTrust("bouncer.home", 6697).PinnedFingerprint,
                Is.EqualTo(Fingerprint));
            Assert.That(saved[0].Servers[0].PinnedFingerprint, Is.EqualTo(Fingerprint),
                "the pin is in servers.json, so it is there after a restart");
        });
    }

    [Test]
    public void PinCertificate_ReplacesAnOldPin()
    {
        var home = new NetworkSettings("Home");
        home.AddServer(new ServerSettings("bouncer.home", 6697, true) { PinnedFingerprint = Fingerprint });
        IrcSettingsManager manager = ManagerWith(home);

        manager.PinCertificate("bouncer.home", 6697, new string('F', 64));

        Assert.That(manager.GetCertificateTrust("bouncer.home", 6697).PinnedFingerprint,
            Is.EqualTo(new string('F', 64)), "an entry point has at most one pin");
    }

    [Test]
    public void PinCertificate_ForAnEntryPointThatNoNetworkLists_PinsNothing()
    {
        var home = new NetworkSettings("Home");
        home.AddServer(new ServerSettings("bouncer.home", 6697, true));
        IrcSettingsManager manager = ManagerWith(home);

        bool pinned = manager.PinCertificate("bouncer.home", 7000, Fingerprint);

        Assert.Multiple(() =>
        {
            Assert.That(pinned, Is.False);
            Assert.That(home.Servers[0].PinnedFingerprint, Is.Null);
            Assert.That(File.Exists(_path), Is.False, "nothing changed, so nothing is saved");
        });
    }
}
