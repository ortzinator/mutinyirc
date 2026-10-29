using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;
using FakeItEasy;
using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.UI.ViewModels;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the "Trust this certificate" button for a host that no saved network lists, for
/// example one dialled with <c>/server</c>. The pin action mints and saves a network named after
/// the host, and at registration that network can merge into a saved network by the name that
/// the server reports.
/// </summary>
[TestFixture]
public class ServerViewModelMintedNetworkTests
{
    private static readonly FieldInfo SettingsSingleton =
        typeof(IrcSettingsManager).GetField("Singleton", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly PropertyInfo ListenerProperties =
        typeof(Listener).GetProperty("ServerProperties", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private const string Host = "self.signed.example";
    private const int Port = 6697;

    /// <summary>
    /// A connection over one TLS entry point, as <c>/server</c> makes it, whose dials only record
    /// the entry point, so a test can refuse a certificate or register at will.
    /// </summary>
    private sealed class FakedServer : Server
    {
        public List<string> Dialled { get; } = new();

        // The base constructor sets it through CreateConnection. IDE0370 is a false positive
        // here: without the '!' the build gives CS8618.
#pragma warning disable IDE0370
        public IConnection Current { get; private set; } = null!;
#pragma warning restore IDE0370

        public FakedServer(ConnectionArgs args) : base(args) { }

        protected override IConnection CreateConnection(ConnectionArgs args)
        {
            var conn = A.Fake<IConnection>();
            var listener = new Listener();
            A.CallTo(() => conn.Listener).Returns(listener);
            A.CallTo(() => conn.ServerProperties).Returns((ServerProperties)ListenerProperties.GetValue(listener)!);
            A.CallTo(() => conn.ConnectionData).Returns(args);
            A.CallTo(() => conn.TryConnect())
                .Invokes(() => Dialled.Add($"{args.Hostname}:{args.Port}"))
                .Returns(true);
            Current = conn;
            return conn;
        }

        public void RefuseCurrent(X509Certificate2 certificate) =>
            Current.ConnectFailed += Raise.With(Current,
                new ConnectFailedEventArgs(ConnectError.CertificateRejected,
                    new RejectedCertificate(certificate.RawData, SslPolicyErrors.RemoteCertificateChainErrors)));

        /// <summary>
        /// Registers the current connection. The 005 that announces the network name comes
        /// first; pass an empty name for a server that announces no network.
        /// </summary>
        public void RegisterCurrent(string reportedNetworkName)
        {
            if (reportedNetworkName != string.Empty)
                Current.Listener.Parse(
                    $":{Host} 005 me NETWORK={reportedNetworkName} :are supported by this server");
            Current.Listener.Parse($":{Host} 001 me :Welcome");
        }
    }

    private X509Certificate2 _certificate = null!;
    private string _settingsPath = null!;
    private IrcSettingsManager _settings = null!;
    private FakedServer _server = null!;
    private ServerViewModel _vm = null!;

    [OneTimeSetUp]
    public void CreateCertificate()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=" + Host, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        _certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    [OneTimeTearDown]
    public void DisposeCertificate() => _certificate.Dispose();

    /// <summary>The fingerprint, worked out apart from the code under test.</summary>
    private string Fingerprint => Convert.ToHexString(SHA256.HashData(_certificate.RawData));

    [SetUp]
    public void Setup()
    {
        _settingsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
        _settings = new IrcSettingsManager(_settingsPath);
        SettingsSingleton.SetValue(null, _settings);
    }

    [TearDown]
    public void Teardown()
    {
        _vm?.Dispose();
        _server?.Dispose();
        SettingsSingleton.SetValue(null, null);
        File.Delete(_settingsPath);
    }

    /// <summary>Dials <see cref="Host"/> as <c>/server</c> does: one entry point, no saved network.</summary>
    private void DialUnsavedHost()
    {
        _server = new FakedServer(new ConnectionArgs("me", Host, true) { Port = Port })
        {
            // As MainViewModel sets it, less the marshal to the UI thread: the tests run there.
            CertificateTrustLookup = (host, port) => _settings.GetCertificateTrust(host, port)
        };
        _vm = new ServerViewModel(_server, new PluginManager());
        _server.Connect();
    }

    private async Task RefuseThenTrust()
    {
        _server.RefuseCurrent(_certificate);
        await _vm.ChatLines.OfType<CertificateRefusalViewModel>().Single().TrustCommand.ExecuteAsync(null);
    }

    private NetworkSettings SaveLibera()
    {
        NetworkSettings libera = _settings.GetOrAddNetwork("Libera");
        libera.AddServer(new ServerSettings("irc.libera.chat", 6697, true));
        return libera;
    }

    [AvaloniaTest]
    public async Task Trust_ForAnUnsavedHost_SavesANetworkNamedAfterTheHostThatHoldsThePin()
    {
        DialUnsavedHost();

        await RefuseThenTrust();

        NetworkSettings minted = _settings.Networks.Single();
        ServerSettings entryPoint = minted.Servers.Single();
        Assert.Multiple(() =>
        {
            Assert.That(minted.Name, Is.EqualTo(Host));
            Assert.That((entryPoint.Url, entryPoint.Port, entryPoint.Ssl), Is.EqualTo((Host, Port, true)));
            Assert.That(entryPoint.PinnedFingerprint, Is.EqualTo(Fingerprint));
            Assert.That(File.ReadAllText(_settingsPath), Does.Contain(Fingerprint), "the network is saved");
        });
    }

    [AvaloniaTest]
    public async Task Trust_ForAnUnsavedHost_DialsItThroughThePin()
    {
        DialUnsavedHost();
        int dials = _server.Dialled.Count;

        await RefuseThenTrust();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Has.Count.EqualTo(dials + 1));
            Assert.That(_server.Dialled.Last(), Is.EqualTo($"{Host}:{Port}"));
            Assert.That(_server.CertificateTrustLookup(Host, Port).IsPinned(Fingerprint), Is.True,
                "the next handshake finds the pin");
            Assert.That(_vm.ChatLines.OfType<CertificateRefusalViewModel>().Single().CanTrust, Is.False);
        });
    }

    [AvaloniaTest]
    public async Task Trust_ForAHostSavedOnAnotherPort_SaysSoAndDialsNothing()
    {
        NetworkSettings home = _settings.GetOrAddNetwork("Home");
        home.AddServer(new ServerSettings(Host, 7000, true));
        DialUnsavedHost();
        int dials = _server.Dialled.Count;

        await RefuseThenTrust();

        Assert.Multiple(() =>
        {
            Assert.That(_server.Dialled, Has.Count.EqualTo(dials), "nothing is dialled");
            Assert.That(_settings.Networks.Single(), Is.SameAs(home), "no network is minted");
            Assert.That(home.Servers.Single().PinnedFingerprint, Is.Null);
            Assert.That(_vm.ChatLines.OfType<ErrorMessageViewModel>().Last().Message, Is.EqualTo(
                "Mutiny cannot pin this certificate, because a saved network lists self.signed.example "
                + "on a port other than 6697. Change the port of that entry point in the Networks dialog, "
                + "then connect again."));
        });
    }

    [AvaloniaTest]
    public async Task AMintedHostThatReportsASavedNetwork_MovesIntoThatNetworkWithItsPin()
    {
        NetworkSettings libera = SaveLibera();
        DialUnsavedHost();
        await RefuseThenTrust();

        _server.RegisterCurrent("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks.Single(), Is.SameAs(libera), "the minted network is gone");
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat", Host }));
            Assert.That(libera.GetServer(Host)!.PinnedFingerprint, Is.EqualTo(Fingerprint));
            Assert.That(libera.GetServer(Host)!.Network, Is.SameAs(libera));
            Assert.That(_vm.NetworkName, Is.EqualTo("Libera"));
        });
    }

    [AvaloniaTest]
    public async Task AMintedHostThatReportsANewName_TakesThatName()
    {
        SaveLibera();
        DialUnsavedHost();
        await RefuseThenTrust();
        NetworkSettings minted = _settings.Networks.Single(n => n.Name == Host);

        _server.RegisterCurrent("SelfNet");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Has.Count.EqualTo(2));
            Assert.That(minted.Name, Is.EqualTo("SelfNet"));
            Assert.That(minted.Servers.Single().PinnedFingerprint, Is.EqualTo(Fingerprint));
        });
    }

    [AvaloniaTest]
    public async Task AMintedHostThatReportsNoName_CanStillMergeOnALaterRegistration()
    {
        NetworkSettings libera = SaveLibera();
        DialUnsavedHost();
        await RefuseThenTrust();

        _server.RegisterCurrent(string.Empty);
        _server.RegisterCurrent("Libera");

        Assert.That(_settings.Networks.Single(), Is.SameAs(libera));
    }

    [AvaloniaTest]
    public async Task AMintedNetworkWithASecondEntryPoint_DoesNotMergeByName()
    {
        NetworkSettings libera = SaveLibera();
        DialUnsavedHost();
        await RefuseThenTrust();
        NetworkSettings minted = _settings.Networks.Single(n => n.Name == Host);
        minted.AddServer(new ServerSettings("other.signed.example", Port, true));

        _server.RegisterCurrent("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Has.Count.EqualTo(2));
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
            Assert.That(minted.Servers.Select(s => s.Url), Is.EqualTo(new[] { Host, "other.signed.example" }));
        });
    }

    [AvaloniaTest]
    public void ANetworkThePinActionDidNotMint_NeverMergesByName()
    {
        NetworkSettings libera = SaveLibera();
        NetworkSettings own = _settings.GetOrAddNetwork("Own");
        own.AddServer(new ServerSettings(Host, Port, true));
        DialUnsavedHost();

        _server.RegisterCurrent("Libera");

        Assert.Multiple(() =>
        {
            Assert.That(_settings.Networks, Has.Count.EqualTo(2));
            Assert.That(libera.Servers.Select(s => s.Url), Is.EqualTo(new[] { "irc.libera.chat" }));
            Assert.That(own.Servers.Select(s => s.Url), Is.EqualTo(new[] { Host }));
        });
    }

    [AvaloniaTest]
    public async Task TheMintedMark_IsSavedUntilTheNetworkRegistersWithAName()
    {
        DialUnsavedHost();
        await RefuseThenTrust();
        Assert.That(File.ReadAllText(_settingsPath), Does.Contain("\"MintedByPin\": true"),
            "a restart still knows that the pin action minted the network");

        _server.RegisterCurrent("SelfNet");
        _settings.Save();

        Assert.That(File.ReadAllText(_settingsPath), Does.Not.Contain("MintedByPin"));
    }
}
