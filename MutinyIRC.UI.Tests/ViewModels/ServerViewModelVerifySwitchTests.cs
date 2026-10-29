using System.Linq;
using System.Net.Security;
using System.Reflection;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers what the server window and the sidebar say about a connection that was accepted only
/// because the network's verify switch is off, and what they say when the certificate passed.
/// </summary>
[TestFixture]
public class ServerViewModelVerifySwitchTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private const string Host = "bouncer.home";
    private const int Port = 6697;

    private Connection _connection = null!;
    private Server _server = null!;
    private ServerViewModel _vm = null!;

    [SetUp]
    public void Setup()
    {
        // Registration files the connection under a network, so give it settings of its own,
        // over a file of this fixture's own, instead of the user's real servers.json.
        SettingsSingleton.SetValue(null, new IrcSettingsManager(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                   System.IO.Path.GetRandomFileName() + ".json")));

        // A real connection that never opens: the test plays the TLS handshake by calling the
        // certificate callback that Server gave it.
        _connection = new Connection(new ConnectionArgs("me", Host, true) { Port = Port }, false, false);
        _server = new Server(_connection);
        _vm = new ServerViewModel(_server, new PluginManager());
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
    }

    /// <summary>Plays a handshake whose certificate fails the normal check.</summary>
    private bool HandshakeWithUntrustedCertificate(bool verify)
    {
        _server.CertificateTrustLookup = _ => new CertificateTrust { VerifyCertificates = verify };
        return _connection.CertificateValidationFailedCallback(
            null, null, SslPolicyErrors.RemoteCertificateChainErrors);
    }

    private void Register() => _connection.Listener.Parse(":bouncer.home 001 me :Welcome");

    [Test]
    public void VerifySwitchOff_ConnectsWithAWarningAndANotVerifiedStatus()
    {
        Assert.That(HandshakeWithUntrustedCertificate(verify: false), Is.True, "Precondition: accepted");

        Register();

        Assert.Multiple(() =>
        {
            Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connected · TLS · not verified"));
            Assert.That(_vm.IsConnectionStatusWarning, Is.True, "the status is drawn in the warning colour");
            Assert.That(_vm.ChatLines.OfType<WarningMessageViewModel>().Select(l => l.Message),
                Is.EqualTo(new[]
                {
                    "Connected to bouncer.home:6697 without checking its certificate. Verification is off for this network."
                }));
        });
    }

    [Test]
    public void VerifySwitchOn_RefusesTheCertificate()
    {
        Assert.That(HandshakeWithUntrustedCertificate(verify: true), Is.False);
    }

    [Test]
    public void ACertificateThatPasses_ConnectsWithTheNormalStatus()
    {
        // The callback never runs for a certificate that passes the normal check, whatever
        // the switch says.
        _server.CertificateTrustLookup = _ => new CertificateTrust { VerifyCertificates = false };

        Register();

        Assert.Multiple(() =>
        {
            Assert.That(_vm.ConnectionStatus, Is.EqualTo("Connected · TLS"));
            Assert.That(_vm.IsConnectionStatusWarning, Is.False);
            Assert.That(_vm.ChatLines.OfType<WarningMessageViewModel>(), Is.Empty);
        });
    }

    [Test]
    public void AfterADrop_TheStatusIsNoLongerAWarning()
    {
        HandshakeWithUntrustedCertificate(verify: false);
        Register();

        _connection.Disconnect(DisconnectReason.SocketError);

        Assert.Multiple(() =>
        {
            Assert.That(_vm.IsConnectionStatusWarning, Is.False);
            Assert.That(_server.CertificateAcceptance, Is.EqualTo(CertificateAcceptance.Trusted),
                "the next handshake decides again");
        });
    }
}
